using System;

namespace Core.Utils;

public static class Arrays {
	public static T[] Create<T>(Func<T> initializer, int length) {
		T[] values = new T[length];
		for (int i = 0; i < length; i++) {
			values[i] = initializer();
		}
		return values;
	}

	public static T[] Create<T>(T defaultValue, int length) {
		T[] values = new T[length];
		for (int i = 0; i < length; i++) {
			values[i] = defaultValue;
		}
		return values;
	}
}
