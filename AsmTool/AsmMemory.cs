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
	public class AsmMemory : IDisposable
	{
		private readonly UInt32 handle;
		private readonly uint address;
		private readonly uint size;

		public AsmMemory(uint address, uint size) {
			this.address = address;
			this.size = size;
			this.handle = AsmIO.MapAsmIO(address, size);
		}

		public unsafe byte[] Read(uint offset, uint size) {
			byte[] buf = new byte[(int)size];
			fixed(byte *bufPtr = buf) {
				// asmedia reads 1 byte at a time, replicate for safety
				for (uint i = 0; i < size; i++) {
					Read(bufPtr + i, offset + i, 1);
				}
			}
			return buf;
		}

		private unsafe void Read(byte *ptr, uint offset, uint size) {
			AsmIO.ReadMEM(handle + offset, size, new IntPtr(ptr));
		}

		public void Dispose() {
			AsmIO.UnmapAsmIO(address, size);
		}
	}
}
