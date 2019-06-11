#region License
/*
 * Copyright (C) 2019 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace AsmTool
{
	public class LinuxNativeIO
	{
		[DllImport("AsmIOLinux.so")]
		public static extern uint LoadAsmIODriver();

		[DllImport("AsmIOLinux.so")]
		public static extern byte PCI_Read_BYTE(uint busNumber, uint deviceNumber, uint functionNumber, uint offset);

		[DllImport("AsmIOLinux.so")]
		public static extern uint PCI_Read_DWORD(uint busNumber, uint deviceNumber, uint functionNumber, uint offset);

		[DllImport("AsmIOLinux.so")]
		public static extern uint Wait_Read_Ready(uint busNumber, uint deviceNumber, uint functionNumber);

		[DllImport("AsmIOLinux.so")]
		public static extern uint Wait_Write_Ready(uint busNumber, uint deviceNumber, uint functionNumber);

		[DllImport("AsmIOLinux.so")]
		public static extern uint ReadCMD(uint busNumber, uint deviceNumber, uint functionNumber, IntPtr pBuf);

		[DllImport("AsmIOLinux.so")]
		public static extern uint WriteCmdALL(uint busNumber, uint deviceNumber, uint functionNumber, uint cmd_reg_byte0, uint cmd_reg_byte1, uint cmd_reg_byte2, uint cmd_dat0, uint cmd_dat1, uint cmd_dat2);
	}



	public class LinuxAsmIO : IAsmIO
	{
		public uint LoadAsmIODriver() {
			return LinuxNativeIO.LoadAsmIODriver();
		}

		public uint MapAsmIO(uint address, uint size) {
			throw new NotImplementedException();
		}

		public byte PCI_Read_BYTE(uint busNumber, uint deviceNumber, uint functionNumber, uint offset) {
			return LinuxNativeIO.PCI_Read_BYTE(busNumber, deviceNumber, functionNumber, offset);
		}

		public uint PCI_Read_DWORD(uint busNumber, uint deviceNumber, uint functionNumber, uint offset) {
			return LinuxNativeIO.PCI_Read_DWORD(busNumber, deviceNumber, functionNumber, offset);
		}

		public uint ReadCMD(uint busNumber, uint deviceNumber, uint functionNumber, IntPtr bufPtr) {
			return LinuxNativeIO.ReadCMD(busNumber, deviceNumber, functionNumber, bufPtr);
		}

		public uint ReadMEM(uint address, uint size, IntPtr bufPtr) {
			throw new NotImplementedException();
		}

		public uint UnloadAsmIODriver() {
			return 1;
		}

		public uint UnmapAsmIO(uint address, uint size) {
			throw new NotImplementedException();
		}

		public uint Wait_Read_Ready(uint busNumber, uint deviceNumber, uint functionNumber) {
			return LinuxNativeIO.Wait_Read_Ready(busNumber, deviceNumber, functionNumber);
		}

		public uint Wait_Write_Ready(uint busNumber, uint deviceNumber, uint functionNumber) {
			return LinuxNativeIO.Wait_Write_Ready(busNumber, deviceNumber, functionNumber);
		}

		public uint WriteCmdALL(uint busNumber, uint deviceNumber, uint functionNumber, uint cmd_reg_byte0, uint cmd_reg_byte1, uint cmd_reg_byte2, uint cmd_dat0, uint cmd_dat1, uint cmd_dat2) {
			return LinuxNativeIO.WriteCmdALL(busNumber, deviceNumber, functionNumber, cmd_reg_byte0, cmd_reg_byte1, cmd_reg_byte2, cmd_dat0, cmd_dat1, cmd_dat2);
		}
	}
}
