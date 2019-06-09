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
	public class Prober
	{
		const uint PCI_BUS_MAX = 256; // 2 ** 8
		const uint PCI_DEV_MAX = 32; //2 ** 5
		const uint PCI_FUNC_MAX = 8; //2 ** 3


		public bool FindByVendor(UInt32 vid, out PCIAddress addr) {
			for(uint i=0; i<PCI_BUS_MAX; i++) {
				for(uint j=0; j<PCI_DEV_MAX; j++) {
					for(uint k=0; k<PCI_FUNC_MAX; k++) {
						uint ident = AsmIO.PCI_Read_DWORD(i, j, k, 0);
						if(ident == 0xFFFFFFFF) {
							continue;
						}
						uint dev_vid = (ident >> 16) & 0xFFFF;
						uint dev_pid = (ident & 0xFFFF);
						Trace.WriteLine($"[bus:{i}, dev:{j}, func:{k}] {dev_vid:X4}:{dev_pid:X4}");

						if(dev_vid == vid) {
							addr = new PCIAddress() {
								Bus = i,
								Device = j,
								Function = k
							};
							return true;
						}
					}
				}
			}
			addr = new PCIAddress() {
				Bus = uint.MaxValue,
				Device = uint.MaxValue,
				Function = uint.MaxValue
			};
			return false;
		}
	}
}
