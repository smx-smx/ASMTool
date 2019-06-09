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
	public enum MemoryType : byte
	{
		Base32 = 0,
		Base64 = 2
	}

	public class PCIBar
	{
		private readonly uint bar;
		public PCIBar(uint bar) {
			this.bar = bar;
		}

		public byte MemorySpace => (byte)(bar & 1);
		public MemoryType Type => (MemoryType)((bar >> 1) & 2);
		public bool IsPrefetchable => ((bar >> 3) & 1) == 1;
		public uint BaseAddress => bar >> 4;
	}
}
