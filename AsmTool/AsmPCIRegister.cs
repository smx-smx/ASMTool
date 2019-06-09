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
	public enum AsmPCIRegister : byte
	{
		RegisterSelect = 0x3E, //0x3E << 2 -> 0xF8
		RegisterData   = 0x3F, //0x3F << 2 -> 0xFC
		ControlRegister = 0x38 //0x38 << 2 -> 0xE0
	}
}
