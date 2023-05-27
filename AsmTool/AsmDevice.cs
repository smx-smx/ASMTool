#region License
/*
 * Copyright (C) 2019 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsmTool
{
	public class AsmDevice {
		const uint PID_2142 = 0x2142;
		const uint PID_3142 = 0x3142;

		const uint FIRMWARE_SIZE = 131072; //128k ROM

		private readonly Prober prb;
		private readonly PCIAddress pcidev;

		/// <summary>
		/// base address register
		/// </summary>
		private readonly PCIBar bar;

		private readonly IAsmIO io;

		public AsmDevice(IAsmIO io) {
			this.io = io;
			this.prb = new Prober(io);

			Console.WriteLine("Scanning for ASMedia ICs...");
			if (!prb.FindByProduct(PID_2142, out pcidev) && !prb.FindByProduct(PID_3142, out pcidev))
				throw new Exception($"No ASMedia device detected!");

			Console.WriteLine("Found ASMedia IC!");
			uint barValue = PCIReadWord(0x10);
			Console.WriteLine($"BAR: {barValue:X8}");

			this.bar = new PCIBar(barValue);
		}

		public AsmMemory NewMemoryMap(uint offset, uint size) {
			return new AsmMemory(io, this.bar.BaseAddress + offset, size);
		}

		public uint PCIReadWord(uint offset) {
			return io.PCI_Read_DWORD(pcidev.Bus, pcidev.Device, pcidev.Function, offset);
		}

		public byte PCIReadByte(uint offset) {
			return io.PCI_Read_BYTE(pcidev.Bus, pcidev.Device, pcidev.Function, offset);
		}

		private static byte[] BuildAsmCommand(
			ASMIOCommand mode, ASMIOFunction function, byte size
		) {
			return new byte[] {
				(byte)mode, (byte)function, size
			};
		}

		private UInt32 WriteRegister(byte[] reg, uint data, uint data2 = 0, uint data3 = 0) {
			return io.WriteCmdALL(pcidev.Bus, pcidev.Device, pcidev.Function,
				reg[0], reg[1], reg[2],
				data, data2, data3
			);
		}

		private bool WaitWrite() {
			if(io.Wait_Write_Ready(pcidev.Bus, pcidev.Device, pcidev.Function) < 0) {
				return false;
			}
			return true;
		}
		
		public unsafe bool WriteMemory(uint address, byte[] bytes) {
			var reg = BuildAsmCommand(ASMIOCommand.Write, ASMIOFunction.Memory, (byte)bytes.Length);


			var span = bytes.AsSpan();
			while(span.Length > 0) {
				var length = Math.Min(8, span.Length);

				ulong value = 0;
				for (int i = 0; i < length; i++) {
					value |= (uint)(span[i] << (i * 8));
				}

				span = span.Slice(length);

				uint word0 = (uint)(value >> 0);
				uint word1 = (uint)(value >> 32);
				Trace.WriteLine($"W0 is {word0}, W1 is {word1}");

				if (WriteRegister(reg, address, word0, word1) < 0) {
					return false;
				}
				if (!WaitWrite()) {
					return false;
				}
			}
						

			return true;
		}

		public unsafe byte[]? ReadMemory(uint address) {
			byte wordSize = 4;
			var reg = BuildAsmCommand(ASMIOCommand.Read, ASMIOFunction.Memory, wordSize);
			if (WriteRegister(reg, address) < 0) {
				Console.WriteLine("WriteRegister failed!");
				return null;
			}

			if (!ReadPacket(wordSize, out byte[]? ack) || ack == null) {
				Console.WriteLine("Failed to read ack!");
				return null;
			}

			byte[] word = new byte[wordSize];
			if (!ReadPacketSmall(wordSize, word)) {
				Console.WriteLine("Failed to read data!");
				return null;
			}
			return word;
		}

		public unsafe bool ReadPacketSmall(int wordSize, byte[] data) {
			if (io.Wait_Read_Ready(pcidev.Bus, pcidev.Device, pcidev.Function) < 0) {
				Console.WriteLine("Wait_Read_Ready failed!");
				return false;
			}

			for (int i = 0; i < wordSize; i++) {
				byte offset = (byte)(0xF0 + i);
				data[i] = io.PCI_Read_BYTE(pcidev.Bus, pcidev.Device, pcidev.Function, offset);
			}
			// signal read end
			io.PCI_Write_BYTE(pcidev.Bus, pcidev.Device, pcidev.Function, 0xE0, 1);

			return true;
		}

		public unsafe bool ReadPacket(uint wordSize, out byte[]? data) {
			data = null;

			if (io.Wait_Read_Ready(pcidev.Bus, pcidev.Device, pcidev.Function) < 0) {
				Console.WriteLine("Wait_Read_Ready failed!");
				return false;
			}

			data = new byte[0x2C];
			fixed (byte* ptr = data) {
				io.ReadCMD(pcidev.Bus, pcidev.Device, pcidev.Function, new IntPtr(ptr));
			}
			return true;
		}

		private static unsafe T? ReadStructure<T>(byte[] data) {
			fixed (byte* ptr = data) {
				return Marshal.PtrToStructure<T>(new IntPtr(ptr));
			}
		}

		public unsafe bool DumpFirmware(string filename) {
			uint num_reads = FIRMWARE_SIZE / 8;

			BinaryWriter bw = new BinaryWriter(new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));
			for (uint i=0; i<num_reads; i++) {
				if (!SPIReadQword(i * 8, out ulong qword)) {
					Console.WriteLine($"Invalid SPI Read at offset {i * 8}");
					return false;
				}
				//Console.WriteLine($"QW: {qword:X16}");
				bw.Write(qword);
				//break;
				//Thread.Sleep(1000);
			}

			bw.Close();
			return true;
		}

		public bool SPIReadQword(uint offset, out ulong qword) {
			qword = 0;

			byte wordSize = 0x8;
			var SPIReg = BuildAsmCommand(ASMIOCommand.Read, ASMIOFunction.Flash, wordSize);
			if (WriteRegister(SPIReg, offset) < 0) {
				Console.WriteLine("WriteRegister failed!");
				return false;
			}

			if(!ReadPacket(wordSize, out byte[]? ack) || ack == null) {
				Console.WriteLine("Failed to read ack!");
				return false;
			}

			if(!ReadPacket(wordSize, out byte[]? reply) || reply == null) {
				Console.WriteLine("Failed to read reply!");
				return false;
			}

			AsmIOPacket pkt = ReadStructure<AsmIOPacket>(reply);
			
			// assemble qword in little endian order due to BinaryWriter being LE only
			qword = ((ulong)(pkt.Data2) << 32) | pkt.Data1;

			return true;
		}

		public void DumpMemory() {
			var MEM_SIZE = 128*1024 ;
			using var fh = new FileStream("mem.bin",
				FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
			fh.SetLength(0);

			var wordSize = 4;
			for (int i = 0; i < MEM_SIZE; i+=wordSize) {
				var data = ReadMemory((uint)i);
				if (data != null) {
					Console.WriteLine("writing");
					fh.Write(data);
				} else {
					break;
				}
			}
		}
	}
}
