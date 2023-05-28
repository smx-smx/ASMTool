#region License
/*
 * Copyright (C) 2023 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using Smx.SharpIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmTool
{
	public enum AsmFirmwareChipType : byte {
		Asm2142 = 0x50,
		Asm3142 = 0x70
	}

	public class AsmFirmware : IDisposable
	{
		private readonly string filePath;
		private readonly MFile mf;
		private readonly SpanStream stream;

		private const string MAGIC_GEN1 = "2114A_RCFG";
		private const string MAGIC_GEN2 = "2214A_RCFG";

		private Span<byte> Span {
			get {
				return mf.Span.Memory.Span;
			}
		}

		public AsmFirmware(string firmwarePath) {
			filePath = firmwarePath;
			mf = MFile.Open(firmwarePath, FileMode.Open,
				FileAccess.Read | FileAccess.Write, FileShare.Read);
			stream = new SpanStream(mf);
		}

		private void UpdateChecksum() {
			Span[HeaderChecksumOffset] = ComputeHeaderChecksum();
			Span[BodyChecksumOffset] = ComputeBodyChecksum();
		}

		public void SetChipType(AsmFirmwareChipType type) {
			Span[0xBC] = (byte)type;
			UpdateChecksum();
		}

		private int GetSignatureType() {
			string magic = ReadStringSignature();
			switch(magic) {
				case MAGIC_GEN1: return 0;
				case MAGIC_GEN2: return 1;
				default:
					throw new InvalidDataException($"Unexpected magic \"{magic}\"");
			}
		}

		private byte[] GetFirmwareVersion() {
			var fwVer = stream.PerformAt(0xB9, () => {
				return stream.ReadBytes(6);
			});
			return fwVer;
		}

		private AsmFirmwareChipType GetFirmwareChipType() {
			var fwVer = GetFirmwareVersion();
			switch (fwVer[3]) {
				case 0x50: return AsmFirmwareChipType.Asm2142;
				case 0x70: return AsmFirmwareChipType.Asm3142;
				default: throw new InvalidDataException($"Unknown chip id {fwVer[3]:X2}");
			}
		}

		private string GetFirmwareVersionString() {
			var fwVer = GetFirmwareVersion();
			return string.Format("{0:X2}{1:X2}{2:X2}_{3:X2}_{4:X2}_{5:X2}",
				// firmware version
				fwVer[0], fwVer[1], fwVer[2],
				// chip type (0x50: 2142, 0x70: 3142)
				fwVer[3],
				// unk (chip sub-type?)
				fwVer[4],
				fwVer[5]
			);
		}

		private string GetFirmwareName() {
			var fwChipName = stream.PerformAt(0xB9 + 7, () => {
				return stream.ReadCString(Encoding.ASCII);
			});
			return fwChipName;
		}

		private string ReadFooterSignature() {
			var offset = HeaderSize + BodySize + 9;
			return stream.PerformAt(offset, () => {
				return stream.ReadString(8, Encoding.ASCII);
			});
		}

		private string ReadStringSignature() {
			return stream.PerformAt(6, () => { 
				return stream.ReadString(10, Encoding.ASCII);
			});
		}

		private ushort HeaderSize {
			get {
				return (ushort)((ushort)0u
					| (ushort)(Span[4] << 0)
					| (ushort)(Span[5] << 8)
				);
			}
		}

		private uint BodySize {
			get {
				return (0u
					| (uint)(Span[HeaderSize + 5] << 0)
					| (uint)(Span[HeaderSize + 6] << 8)
					| (uint)(Span[HeaderSize + 7] << 16)
				);
			}
		}

		private int HeaderChecksumOffset => HeaderSize;
		private int BodyChecksumOffset => (int)(HeaderSize + BodyStartOffset + BodySize + 8);

		
		private byte HeaderChecksum => Span[HeaderChecksumOffset];
		private byte BodyChecksum => Span[BodyChecksumOffset];

		private int BodyStartOffset {
			get {
				if (ReadStringSignature() == MAGIC_GEN1) return 7;
				return 9;
			}
		}

		private byte ComputeBodyChecksum() {
			var body_start_offset = BodyStartOffset;

			byte p0 = 0;
			byte p1 = 0;
			int i = 0;

			var body_start = HeaderSize + body_start_offset;
			if (BodySize >= 2) {
				for (i = 0; i < BodySize; i += 2) {
					p0 += Span[body_start + i];
					p1 += Span[body_start + i + 1];
				}
			}

			byte p2;
			if (i >= BodySize) {
				p2 = 0;
			} else {
				p2 = Span[body_start + i];
			}

			byte checksum = 0;
			checksum += p0;
			checksum += p1;
			checksum += p2;
			return checksum;
		}

		private byte ComputeHeaderChecksum() {
			byte p0 = 0;
			byte p1 = 0;
			int i = 0;
			if (HeaderSize >= 2) {
				for (i = 0; i < HeaderSize; i += 2) {
					p0 += Span[i];
					p1 += Span[i + 1];
				}
			}

			byte p2;
			if (i >= HeaderSize) {
				p2 = 0;
			} else {
				p2 = Span[i];
			}

			byte checksum = 0;
			checksum += p0;
			checksum += p1;
			checksum += p2;
			return checksum;

		}

		public void PrintInfo(AsmDevice dev, TextWriter os) {
			os.WriteLine("==== File Info ====");
			os.WriteLine($"File: {filePath}");

			var compHeaderChecksum = ComputeHeaderChecksum();
			var compBodyChecksum = ComputeBodyChecksum();

			os.WriteLine($"File Checksum [header]: {HeaderChecksum:X2}");
			os.WriteLine($"File Checksum [body]: {BodyChecksum:X2}");

			os.WriteLine($"Computed Checksum [header]: {compHeaderChecksum:X2}");
			os.WriteLine($"Computed Checksum [body]: {compBodyChecksum:X2}");

			if (HeaderChecksum != compHeaderChecksum) {
				os.WriteLine("!! WARNING: Checksum Mismatch");
			}

			os.WriteLine($"Signature: " + ReadStringSignature());
			os.WriteLine($"FW Name: " + GetFirmwareName());
			var fwChipType = GetFirmwareChipType();
			var fwChipName = fwChipType switch {
				AsmFirmwareChipType.Asm2142 => "ASM2142",
				AsmFirmwareChipType.Asm3142 => "ASM3142",
				_ => "Unknown"
			};
			os.WriteLine($"Footer: " + ReadFooterSignature());

			os.WriteLine($"Chip: 0x{(byte)fwChipType:X2}: {fwChipName}");

			os.WriteLine("==== Actual Chip Info ====");

			var chipRev0 = dev.ReadMemory(0x150B2)?[0];
			var chipRev1 = dev.ReadMemory(0xF38C)?[0];

			if(chipRev0 != null) {
				Console.WriteLine($"Chip Rev0: 0x{chipRev0:X2}");
			}
			if(chipRev1 != null) {
				Console.WriteLine($"Chip Rev1: 0x{chipRev1:X2}");
			}

		}

		public void Dispose() {
			mf.Dispose();
		}
	}
}
