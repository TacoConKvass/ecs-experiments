using Core.DataStructures;
using System.Collections;

namespace Core.ECS;

public class World(int componentCount, byte worldID) {

	internal Entity[] entities = new Entity[128];
	
	internal BitSet[] entityFlags = BitSet.DefaultArray(componentCount, 128);

	public byte WorldID = worldID;

	public int ComponentCount = componentCount;

	public SparseSet<T> GetComponent<T>() where T : struct {
		return Component<T>.internalSet[WorldID];
	}

	public ref Entity GetEntity(int id) {
		EnsureCapacity(id);
		ref Entity result = ref entities[id];
		result.WorldID = WorldID;
		result.ID = id;
		return ref result;
	}

	public bool EntityHas<T>(int id) where T : struct {
		EnsureCapacity(id);
		return entityFlags[id].Has(Component<T>.ID[WorldID]);
	}

	public void Set<T>(int id, T data) where T : struct {
		EnsureCapacity(id);
		if (!EntityHas<T>(id)) Component<T>.Add(id, data, WorldID);
		else Component<T>.Set(id, data, WorldID);
		entityFlags[id].Set(Component<T>.ID[WorldID], true);
	}

	internal void EnsureCapacity(int id) {
		if (id < entityFlags.Length) return;

		int newSize = Math.Max(id, entityFlags.Length * 2);
		int oldSize = entityFlags.Length;
		Array.Resize(ref entities, newSize);
		Array.Resize(ref entityFlags, newSize);
		Array.Copy(BitSet.DefaultArray(ComponentCount, newSize - oldSize), 0, entityFlags, oldSize, newSize - oldSize);
	}

	internal bool QueryByMask(byte[] mask) {
		ReadOnlySpan<byte> maskSpan = mask;

		for (int i = 0; i < entityFlags.Length; i++) {

		}

		return true;
	}
}