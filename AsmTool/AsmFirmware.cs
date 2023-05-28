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

		public AsmFirmware(string firmwarePath) {
			filePath = firmwarePath;
			mf = MFile.Open(firmwarePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			stream = new SpanStream(mf);
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

		private string ReadStringSignature() {
			return stream.PerformAt(6, () => { 
				return stream.ReadString(10, Encoding.ASCII);
			});
		}

		public void PrintInfo(AsmDevice dev, TextWriter os) {
			var mem = mf.Span.Memory;
			var fwVer = GetFirmwareVersionString();

			os.WriteLine("==== Firmware Info ====");
			os.WriteLine($"File: {filePath}");
			os.WriteLine($"Signature: " + ReadStringSignature());
			os.WriteLine($"FW Name: " + GetFirmwareName());
			var fwChipType = GetFirmwareChipType();
			var fwChipName = fwChipType switch {
				AsmFirmwareChipType.Asm2142 => "ASM2142",
				AsmFirmwareChipType.Asm3142 => "ASM3142",
				_ => "Unknown"
			};
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
