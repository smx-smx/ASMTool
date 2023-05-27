#region License
/*
 * Copyright (C) 2019 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using System;
namespace AsmTool
{
	public interface IAsmIO
	{
		UInt32 LoadAsmIODriver();
		UInt32 UnloadAsmIODriver();
		UInt32 ReadMEM(UInt32 address, UInt32 size, IntPtr bufPtr);
		byte PCI_Read_BYTE(UInt32 busNumber, UInt32 deviceNumber, UInt32 functionNumber, UInt32 offset);
		UInt32 PCI_Write_BYTE(UInt32 busNumber, UInt32 deviceNumber, UInt32 functionNumber, UInt32 offset, byte value);
		UInt32 PCI_Read_DWORD(UInt32 busNumber, UInt32 deviceNumber, UInt32 functionNumber, UInt32 offset);

		UInt32 ReadCMD(UInt32 busNumber, UInt32 deviceNumber, UInt32 functionNumber, IntPtr bufPtr);

		UInt32 WriteCmdALL(
			UInt32 busNumber,
			UInt32 deviceNumber,
			UInt32 functionNumber,
			UInt32 cmd_reg_byte0,
			UInt32 cmd_reg_byte1,
			UInt32 cmd_reg_byte2,
			UInt32 cmd_dat0,
			UInt32 cmd_dat1,
			UInt32 cmd_dat2);

		UInt32 Wait_Write_Ready(UInt32 busNumber, UInt32 deviceNumber, UInt32 functionNumber);
		UInt32 Wait_Read_Ready(UInt32 busNumber, UInt32 deviceNumber, UInt32 functionNumber);
		UInt32 MapAsmIO(UInt32 address, UInt32 size);
		UInt32 UnmapAsmIO(UInt32 address, UInt32 size);
	}
}
