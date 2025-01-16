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

    public static void Fill2DArray<T>(T[,] array, T value)
    {
        Random rnd = new Random();
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                array[i, j] = value;
            }
        }
    }
}
