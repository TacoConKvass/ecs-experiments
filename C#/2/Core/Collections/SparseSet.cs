using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ECS.Collections;

public class SparseSet<T> : SparseSetBase where T : struct {
    public int[] Sparse = [];
    public List<int> Dense = [];
    public List<T> Data = [];

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_items")]
    private extern static ref T[] GetArray(List<T> list);

    internal T? _TryGet(int index) {
        if (index > Sparse.Length) return null;
        if (Sparse[index] == -1) return null;

        return Data[Sparse[index]];
    }

    internal ref T GetRef(int index) {
        if (index > Sparse.Length) throw new ArgumentOutOfRangeException("Entity index", "Only access by ref if you are sure the entity has the component");
        if (Sparse[index] == -1) throw new ArgumentOutOfRangeException("Entity index", "Only access by ref if you are sure the entity has the component");

        return ref GetArray(Data)[Sparse[index]];
    }

    internal void _Set(int index, T data) {
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

    internal void _Delete(int index) {
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

    public override object? TryGet(int index) => _TryGet(index);
    
    public override void Set(int index, object value) => _Set(index, (T)value);
    
    public override void Delete(int index) => _Delete(index);
}

public abstract class SparseSetBase {
    public abstract object? TryGet(int index);
    public abstract void Set(int index, object value);
    public abstract void Delete(int index);
}
