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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmTool
{
	class Program
	{
		static void Main(string[] args) {
			IAsmIO io = AsmIOFactory.GetAsmIO();

			Console.WriteLine("Unloading ASM Driver...");
			io.UnloadAsmIODriver();
			Console.WriteLine("Loading ASM Driver...");
			if(io.LoadAsmIODriver() != 1) {
				Console.Error.WriteLine("Failed to load ASM IO Driver");
				return;
			}

			AsmDevice dev = new AsmDevice(io);

			if (args.Length > 0) {
				switch (args[0]) {
					case "mem_read":
						dev.DumpMemory();
						break;
					case "flash_read":
					default:
						Console.WriteLine("Dumping firmware...");
						dev.DumpFirmware("dump.bin");
						break;
				}
			}

			io.UnloadAsmIODriver();
		}
	}
}
