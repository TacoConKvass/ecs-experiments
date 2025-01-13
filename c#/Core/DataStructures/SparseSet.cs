using Core.Utils;
using System;
using System.Runtime.CompilerServices;

namespace Core.DataStructures;

public sealed class SparseSet<T>(int initialCapacity) {
	public int Count { get; set; } = 0;

	public int[] Sparse = Arrays.Create(-1, initialCapacity);

	public int[] Dense = Arrays.Create(-1, initialCapacity);

	public T[] Data = Arrays.Create<T>(default(T), initialCapacity);

	public void Add(int id, T data) {
		EnsureSparseCapacity(id + 1);
		EnsureDataCapacity(Count);

		Sparse[id] = Count;
		Dense[Count] = id;	
		Data[Count] = data;

		Count++;
	}

	public void Set(int id, T data) {
		EnsureSparseCapacity(id + 1);
		EnsureDataCapacity(Sparse[id] + 1);

		if (Sparse[id] == -1) Add(id, data);
		else Data[Sparse[id]] = data;
	}
	
	public void Remove(int id) {
		EnsureSparseCapacity(id + 1);
		EnsureDataCapacity(Sparse[id] + 1);
	}
	
	public ref T Get(int id) {
		EnsureSparseCapacity(id + 1);
		EnsureDataCapacity(Sparse[id] + 1);

		return ref Data[Sparse[id]];
	}
	
	public void EnsureSparseCapacity(int size) {
		if (size < Sparse.Length) return;

		int newLength = Sparse.Length * 2;
		int oldLength = Sparse.Length;

		Array.Resize(ref Sparse, newLength);
		Array.Fill(Sparse, -1, oldLength, newLength - oldLength);
	}
	
	public void EnsureDataCapacity(int size) {
		if (size < Data.Length) return;

		int newLength = Data.Length * 2;
		int oldLength = Data.Length;

		Array.Resize(ref Dense, newLength);
		Array.Fill(Dense, -1, oldLength, newLength - oldLength);

		Array.Resize(ref Data, newLength);
		if (RuntimeHelpers.IsReferenceOrContainsReferences<T>()) Array.Fill(Data, default, oldLength, newLength - oldLength);
	}
}