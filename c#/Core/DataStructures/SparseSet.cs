using System;
using System.Runtime.CompilerServices;

namespace Core.DataStructures;

public struct SparseSet<T> {
	public int Count { get; private set; }

	public int[] Sparse;

	public int[] Dense;

	public T[] Data;

	public SparseSet(int initialCapacity) {
		Sparse = new int[initialCapacity];
		Dense = new int[initialCapacity];
		Data = new T[initialCapacity];

		Array.Fill(Sparse, -1);
		Array.Fill(Dense, -1);
		Array.Fill(Data, default);
	}

	public bool Add(int id, T data) {
		EnsureSparseCapacity(id);
		EnsureDataCapacity(Count);
        if (Sparse[id] > 0) return false;

		Console.WriteLine(Count);
        Sparse[id] = Count;
		Dense[Count] = id;
		Data[Count] = data;
        Count++;
		Console.WriteLine(Count);

		return true;
    }

	public void Set(int id, T data) {
        EnsureSparseCapacity(id);
		EnsureDataCapacity(Sparse[id]);

		if (Sparse[id] == -1) Add(id, data);
		else Data[id] = data;
	} 

	public ref T Get(int id) {
		EnsureSparseCapacity(id);
		EnsureDataCapacity(Sparse[id]);
		return ref Data[Sparse[id]];
	}

	public void Remove(int id) {
		int index = Sparse[id];

        Sparse[Dense[id]] = -1;

        Data[index] = Data[Count];
		if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) Data[Count] = default;

		Dense[index] = Sparse[Dense[Count]];
		Dense[Count] = -1;

		Count--;

		ShrinkIfApplicable();
	}

	private void EnsureSparseCapacity(int id) {
		if (id < Sparse.Length) return;

		Console.WriteLine("Resizin' sparse");
		int newSize = Math.Max(id + 1, Sparse.Length * 2);
		int oldLength = Sparse.Length;

		Array.Resize(ref Sparse, newSize);
        Array.Fill(Sparse, -1, oldLength, newSize - oldLength);
    }

	private void EnsureDataCapacity(int id) {
		if (id + 1 < Data.Length) return;

		Console.WriteLine("Resizin' data");
		int newSize = Math.Max(id + 1, Dense.Length * 2); ;

		Array.Resize(ref Dense, newSize);
		Array.Fill(Dense, -1, Count, newSize - Count);

		Array.Resize(ref Data, newSize);
		if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) Array.Fill(Data, default, Count, newSize - Count);
	}

	private void ShrinkIfApplicable() {
		if (Count >= Data.Length / 3) return;

		int newSize = Data.Length / 2;
		
		Array.Resize(ref Dense, newSize);
		Array.Resize(ref Data, newSize);
	}
}