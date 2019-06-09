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
using System.Text;
using System.Threading.Tasks;

namespace AsmTool
{
	class Program
	{
		static void Main(string[] args) {
			AsmIO.UnloadAsmIODriver();
			if(AsmIO.LoadAsmIODriver() != 1) {
				Console.Error.WriteLine("Failed to load ASM IO Driver");
				return;
			}


			AsmDevice dev = new AsmDevice();
			dev.DumpFirmware("dump.bin");
			
		}
	}
}
