namespace Core.DataStructures;

public class SparseSet<T> {
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
		EnsureDataCapacity();
        if (Sparse[id] > 0) return false;

        Sparse[id] = Count;
		Dense[Count] = id;
		Data[Count] = data;
        Count++;

		return true;
    }

	public void Set(int id, T data) {
        EnsureSparseCapacity(id);
        EnsureDataCapacity();

        Data[id] = data;
	} 

	public void Remove(int id) {
		int index = Sparse[id];

        Sparse[Dense[id]] = -1;

        Data[index] = Data[Count];
		Data[Count] = default;

		Dense[index] = Sparse[Dense[Count]];
		Dense[Count] = -1;

		Count--;

		ShrinkIfApplicable();
	}

	private void EnsureSparseCapacity(int id) {
		if (id < Sparse.Length) return;
		
		int newSize = Sparse.Length;
		int oldLength = Sparse.Length;
		while (newSize <= id) newSize *= 2;

		Array.Resize(ref Sparse, newSize);
        Array.Fill(Sparse, -1, oldLength, newSize - oldLength);
    }

	private void EnsureDataCapacity() {
		if (Count + 1 < Data.Length) return;

		int newSize = Dense.Length * 2;

		Array.Resize(ref Dense, newSize);
		Array.Fill(Dense, -1, Count, newSize - Count);

		Array.Resize(ref Data, newSize);
		Array.Fill(Data, default, Count, newSize - Count);
	}

	private void ShrinkIfApplicable() {
		if (Count >= Data.Length / 3) return;

		int newSize = Data.Length / 2;
		
		Array.Resize(ref Dense, newSize);
		Array.Resize(ref Data, newSize);
	}
}