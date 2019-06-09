#region License
/*
 * Copyright (C) 2019 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
﻿namespace AsmTool
{
	/// <summary>
	/// byte0 of internal register
	/// </summary>
	public enum AsmIORegister : uint
	{
		SPIRead = 0x40,
		SPIWrite = 0x1E
	}
}