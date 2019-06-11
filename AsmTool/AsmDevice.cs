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
		const uint VID_2142 = 0x2142;
		const uint VID_3142 = 0x3142;

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
			if (!prb.FindByVendor(VID_2142, out pcidev) && !prb.FindByVendor(VID_3142, out pcidev))
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

		private static uint ComputeInternalRegister(AsmIORegister r0, byte r1, byte r2) {
			return (uint)((byte)r0 + ((r1 + (r2 << 8)) << 8));
		}

		private UInt32 WriteRegister(uint register, uint data) {
			return io.WriteCmdALL(pcidev.Bus, pcidev.Device, pcidev.Function,
				(register & 0xFF),
				(register >> 8) & 0xFF,
				(register >> 16) & 0xFF,
				data,
				0,
				0
			);
		}

		public unsafe bool ReadPacket(out byte[] data) {
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

		private static unsafe T ReadStructure<T>(byte[] data) {
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

		public unsafe bool SPIReadQword(uint offset, out ulong qword) {
			qword = 0;

			uint SPIReg = ComputeInternalRegister(AsmIORegister.SPIRead, 0x10, 0x8);
			if (WriteRegister(SPIReg, offset) < 0) {
				Console.WriteLine("WriteRegister failed!");
				return false;
			}

			if(!ReadPacket(out byte[] ack)) {
				Console.WriteLine("Failed to read ack!");
				return false;
			}

			if(!ReadPacket(out byte[] reply)) {
				Console.WriteLine("Failed to read reply!");
				return false;
			}

			AsmIOPacket pkt = ReadStructure<AsmIOPacket>(reply);
			
			// assemble qword in little endian order due to BinaryWriter being LE only
			qword = ((ulong)(pkt.Data2) << 32) | pkt.Data1;

			return true;
		}
	}
}
