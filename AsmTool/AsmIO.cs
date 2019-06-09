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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AsmTool
{
	public struct AsmIOPacket
	{
		/*  0 */ public PCIAddress Dev;
		/* 12 */ public UInt32 Data1;
		/* 16 */ public UInt32 Data2;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
		/* 20 */ public byte[] Unknown;
	}

	public class AsmIO
	{
		[DllImport("asmiodll", CallingConvention = CallingConvention.StdCall, EntryPoint = "_LoadAsmIODriver@0", ExactSpelling = true)]
		public static extern UInt32 LoadAsmIODriver();

		[DllImport("asmiodll", CallingConvention = CallingConvention.StdCall, EntryPoint = "_UnloadAsmIODriver@0", ExactSpelling = true)]
		public static extern UInt32 UnloadAsmIODriver();

		[DllImport("asmiodll", CallingConvention = CallingConvention.StdCall, EntryPoint = "_ReadMEM@12", ExactSpelling = true)]
		public static extern UInt32 ReadMEM(UInt32 address, UInt32 size, IntPtr bufPtr);

		[DllImport("asmiodll", CallingConvention = CallingConvention.StdCall, EntryPoint = "_PCI_Read_BYTE@16", ExactSpelling = true)]
		public static extern byte PCI_Read_BYTE(UInt32 busNumber, UInt32 deviceNumber, UInt32 functionNumber, UInt32 offset);

		[DllImport("asmiodll", CallingConvention = CallingConvention.StdCall, EntryPoint = "_PCI_Read_DWORD@16", ExactSpelling = true)]
		public static extern UInt32 PCI_Read_DWORD(UInt32 busNumber, UInt32 deviceNumber, UInt32 functionNumber, UInt32 offset);


		[DllImport("asmiodll", CallingConvention = CallingConvention.StdCall, EntryPoint = "_ReadCMD@16", ExactSpelling = true)]
		public static extern UInt32 ReadCMD(UInt32 busNumber, UInt32 deviceNumber, UInt32 functionNumber, IntPtr bufPtr);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="busNumber">PCI Bus number</param>
		/// <param name="deviceNumber">PCI Device number</param>
		/// <param name="functionNumber">PCI Function number</param>
		/// <param name="cmd_reg_byte0">Byte0 of internal register (low byte - selects function?)</param>
		/// <param name="cmd_reg_byte1">Byte1 of internal register (middle byte - selects device?) </param>
		/// <param name="cmd_reg_byte2">Byte2 of internal register (high byte - selects function?)</param>
		/// <param name="cmd_dat0">Data to write to the internal register</param>
		/// <param name="cmd_dat1">Extra data</param>
		/// <param name="cmd_dat2">Extra data<param>
		/// <returns></returns>
		[DllImport("asmiodll", CallingConvention = CallingConvention.StdCall, EntryPoint = "_WriteCmdALL@36", ExactSpelling = true)]
		public static extern UInt32 WriteCmdALL(
			/* a0 */ UInt32 busNumber, //ioctl arg 0
			/* a1 */ UInt32 deviceNumber, //ioctl arg 1
			/* a2 */ UInt32 functionNumber, //ioctl arg 2
			
			// ioctl arg3, arg4 are reserved for computation of the internal register

			/* a3 */ UInt32 cmd_reg_byte0, //ioctl arg 5
			/* a4 */ UInt32 cmd_reg_byte1, //ioctl arg 6
			/* a5 */ UInt32 cmd_reg_byte2, //ioctl arg 7, also word size
			/* a6 */ UInt32 cmd_dat0, //ioctl arg 8
			/* a7 */ UInt32 cmd_dat1, //ioctl arg 9
			/* a8 */ UInt32 cmd_dat2 //ioctl arg 10
		);

		[DllImport("asmiodll", CallingConvention = CallingConvention.StdCall, EntryPoint = "_Wait_Write_Ready@12", ExactSpelling = true)]
		public static extern UInt32 Wait_Write_Ready(UInt32 busNumber, UInt32 deviceNumber, UInt32 functionNumber);

		[DllImport("asmiodll", CallingConvention = CallingConvention.StdCall, EntryPoint = "_Wait_Read_Ready@12", ExactSpelling = true)]
		public static extern UInt32 Wait_Read_Ready(UInt32 busNumber, UInt32 deviceNumber, UInt32 functionNumber);


		[DllImport("asmiodll", CallingConvention = CallingConvention.StdCall, EntryPoint = "_MapAsmIO@8", ExactSpelling = true)]
		public static extern UInt32 MapAsmIO(UInt32 address, UInt32 size);

		[DllImport("asmiodll", CallingConvention = CallingConvention.StdCall, EntryPoint = "_UnmapAsmIO@8", ExactSpelling = true)]
		public static extern UInt32 UnmapAsmIO(UInt32 address, UInt32 size);

	}
}
