using Core.DataStructures;
using System;

namespace Core.ECS;

public struct Entity(int id, BitSet flags) {
	public int ID = id;

	public BitSet Flags = flags;

	public bool Has<T>() where T : struct => Flags.Has(Component<T>.Data[ECS.ActiveWorld.ID].Offset);

	public Entity Set<T>(T data) where T : struct {
		var component = Component<T>.Data[ECS.ActiveWorld.ID];
		Flags.Set(component.Offset, true);
		component.DataStore.Set(ID, data);
		return this;
	}
}

public class World(int worldID) {
	public int ID = worldID;

	public BitSet[] Entities = [];

	int componentCount = 0;

	public World RegisterComponent<T>() where T : struct {
		Component<T>.AddToWorld(ID, componentCount++);
		return this;
	}

	public ref (SparseSet<T> DataStore, int Offset, bool Dirty) GetComponent<T>() where T : struct {
		return ref Component<T>.Data[ID];
	}

	public World Initialise() {
		Entities = new BitSet[128];
		for (int i = 0; i < 128; i++) {
			Entities[i] = new BitSet(componentCount);
		}
		ECS.SetActiveWorld(this);
		return this;
	}

	public Entity GetEntity(int id) {
		EnsureCapacity(id);
		return new Entity(id, Entities[id]);
	}

	internal void EnsureCapacity(int length) {
		if (Entities.Length > length) return;

		int oldLength = Entities.Length;
		int newSize = Math.Max(length, Entities.Length * 2);

		Array.Resize(ref Entities, newSize);

		for (int i = oldLength; i < newSize; i++) {
			Entities[i] = new BitSet(componentCount);
		}
	}
}