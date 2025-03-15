using System;
using System.Collections;
using System.Collections.Generic;

namespace ECS.Collections;

public class SparseSet<T> where T : struct {
	public int[] Sparse = [];
	public List<int> Dense = [];
	public List<T> Data = [];

	public T? TryGet(int index) {
		if (index > Sparse.Length) return null;
		if (Sparse[index] == -1) return null;

		return Data[Sparse[index]];
	}

	public void Set(int index, T data) {
		if (index >= Sparse.Length) {
			int old_length = Sparse.Length;
			Array.Resize(ref Sparse, Math.Max(index + 1, Sparse.Length * 2));
			Array.Fill(Sparse, -1, old_length, Sparse.Length - old_length);
		}

		if (Sparse[index] == -1) {
			Data.Add(data);
			Dense.Add(index);
			Sparse[index] = Data.Count - 1;
			return;
		}

		Data[Sparse[index]] = data;
		Dense[Sparse[index]] = index;
	}

	public void Delete(int index) {
		if (index >= Sparse.Length) return;
		if (Sparse[index] == -1) return;
		
		int sparse_index = Sparse[index];

		int last_sparse = Dense[Dense.Count - 1];
		Dense[index] = Sparse[last_sparse];
		Data[index] = Data[Sparse[last_sparse]];
		Sparse[last_sparse] = index;

		Data.RemoveAt(Data.Count - 1);
		Dense.RemoveAt(Dense.Count - 1);
		Sparse[index] = -1;
	}
}