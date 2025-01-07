using Core.DataStructures;

namespace Core.ECS;

public class ECS {
    private static int componentCount = 0;
	internal byte[] temp;
	internal byte[][] entityFlags;
	public ECS(int componentCount) {
		var result = Math.DivRem(componentCount, 8);
		int count = result.Quotient + (result.Remainder == 0 ? 0 : 1);
		entityFlags = new byte[count][];
		temp = new byte[count];
		Array.Fill<byte>(temp, 0);
		Array.Fill(entityFlags, temp);
	}

	public static ECS InitialiseWorld() => new ECS(componentCount);
	public static void RegisterComponent<T>() where T : struct {
		if (Component<T>.ID > 0) return;
		Component<T>.ID = componentCount++;
	}

	public bool EntityHas<T>(int id) where T : struct {
		var result = Math.DivRem(Component<T>.ID, 8);
		return (entityFlags[id][result.Quotient] & 1 << result.Remainder) > 0;
	}

	internal void SetEntityFlag<T>(int id) where T : struct {
		var result = Math.DivRem(Component<T>.ID, 8);
		entityFlags[id][result.Quotient] |= (byte)(1 << result.Remainder);
	}

	public void AddToEntity<T>(int id, T data) where T : struct {
		EnsureCapacity(id);
		Component<T>.Add(id, data);
		SetEntityFlag<T>(id);
	}

	internal void EnsureCapacity(int id) {
		if (id < entityFlags.Length) return;

		int newSize = entityFlags.Length;
		int oldLength = entityFlags.Length;
		while (newSize <= id) newSize *= 2;

		Array.Resize(ref entityFlags, newSize);
		Array.Fill(entityFlags, temp, oldLength, newSize - oldLength);
	}
}

public static class Component<T> where T : struct {
	public static int ID = -1;
	private static SparseSet<T> internalSet = new SparseSet<T>(128);
	public static ref int[] Sparse => ref internalSet.Sparse;
	public static ref int[] Dense => ref internalSet.Dense;
	public static ref T[] Data => ref internalSet.Data;

	public static void Add(int id, T data) {
		internalSet.Add(id, data);
	}
}