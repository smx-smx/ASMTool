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
namespace AsmTool
{
	public class AsmIOFactory
	{
		public static IAsmIO GetAsmIO() {
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
				return new LinuxAsmIO();
			} else if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				return new WindowsAsmIO();
			} else {
				throw new NotSupportedException("This Operating System is currently not supported");
			}
		}
	}
}
