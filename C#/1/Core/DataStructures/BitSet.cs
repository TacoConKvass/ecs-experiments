﻿using System;

namespace Core.DataStructures;

public struct BitSet(int flagCount) {
	const int BITS_IN_BYTE = 8;

	internal readonly byte[] Flags = new byte[(flagCount / BITS_IN_BYTE) + 1];

	public bool Has(int flag) {
		int offset = Math.DivRem(flag, BITS_IN_BYTE, out int remainder);

		return (Flags[offset] & 1 << remainder) > 0;
	}

	public void Set(int flag, bool value) {
		int offset = Math.DivRem(flag, BITS_IN_BYTE, out int remainder);

		if (value) Flags[offset] |= (byte)(1 << remainder);
		else Flags[offset] &= (byte)~(1 << remainder);
	}
}
