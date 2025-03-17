using ECS.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ECS;

public class World {
	public struct EntityMap {
		internal BitArray[] componentFlags;
		internal World world;

		public Entity this[int index] {
			get => new Entity(world, index);
			set => new Entity(world, index).CopyFrom(value);
		} 
	}

	internal static int lastWorldId = -1;

	internal int nextEntityId = 0;
	internal Stack<int> freeEntityIds;
	internal List<SparseSetBase> components = [];
	internal List<Type> componentTypes = [];

	public int Id;
	public int ComponentCount;
	public EntityMap Entities;

	public World(int initial_size = 256) {
		Id = ++lastWorldId;
		ComponentCount = 0;
		freeEntityIds = new Stack<int>();

		Entities = new EntityMap() {
			componentFlags = [],
			world = this,
		};

		Array.Resize(ref Entities.componentFlags, initial_size);
		for (int i = 0; i < initial_size; i++) Entities.componentFlags[i] = new BitArray(ComponentCount);
	}

	public void AddComponent<T>() where T : struct {
		ComponentStorage<T>.AddTo(this);
		components.Add(GetComponent<T>().Data);
		componentTypes.Add(typeof(T));
		ComponentCount++;
		
		for (int i = 0; i < Entities.componentFlags.Length; i++) {
			Entities.componentFlags[i].Length = ComponentCount;
		}
	}

	public ComponentRecord GetComponent<T>() where T : struct => ComponentStorage<T>.GetFrom(this);

	internal int GetFreeId() {
		int freeId = freeEntityIds.TryPop(out int id) ? id : nextEntityId++;
		
		if (freeId >= Entities.componentFlags.Length) {
			int old_length = Entities.componentFlags.Length;
			
			Array.Resize(ref Entities.componentFlags, Math.Max(freeId, old_length * 2));
			for (int i = old_length; i < Entities.componentFlags.Length; i++) Entities.componentFlags[i] = new BitArray(ComponentCount);
		}

		return freeId;
	}

	// public IEnumerator<Entity> Query<TWith>() {	}

	// public IEnumerator<Entity> Query<TWith, TWithout>() { }
}

public ref struct Entity {
	public readonly int Id;
	
	internal readonly World world;
	internal ref BitArray componentFlags;
	
	public Entity(World e_world, int? id = null) {
		Id = id ?? e_world.GetFreeId();
		world = e_world;
		componentFlags = ref world.Entities.componentFlags[Id];
	}

	public bool Has<T>() where T : struct {
		int index = world.GetComponent<T>().Id;
		return componentFlags[index];
	}

	public T? Get<T>() where T : struct {
		return (T?)world.GetComponent<T>().Data.TryGet(Id);
	}

	public void Set<T>(T data) where T : struct {
		ComponentRecord component = world.GetComponent<T>();
		componentFlags[component.Id] = true;
		component.Data.Set(Id, data);
	}

	public void Remove<T>() where T : struct {
		ComponentRecord component = world.GetComponent<T>();
		componentFlags[component.Id] = false;
		component.Data.Delete(Id);
	}

	public void Destroy() {
		if (componentFlags.HasAnySet()) {
			foreach (SparseSetBase component in world.components) component.Delete(Id);
		}

		componentFlags.SetAll(false);
		world.freeEntityIds.Push(Id);
	}

	public void CopyFrom(Entity incoming) {
		for (int i = 0; i < world.ComponentCount; i++) {
			if (!incoming.componentFlags[i]) continue;

			world.components[i].Set(Id, incoming.world.components[i].TryGet(incoming.Id));
		}
	}
}

public static class ComponentStorage<T> where T : struct {
	internal static int[] id = [];
	internal static SparseSet<T>?[] components = [];

	internal static void AddTo(World world) {
		if (world.Id >= id.Length) {
			int old_length = id.Length;
			Array.Resize(ref id, World.lastWorldId + 1);
			Array.Fill(id, -1, id.Length - old_length, old_length);
			Array.Resize(ref components, World.lastWorldId + 1);
		}

		id[world.Id] = world.ComponentCount;
		components[world.Id] = new SparseSet<T>();
	}

	internal static ComponentRecord GetFrom(World world) {
		return new ComponentRecord(id[world.Id], components[world.Id]);
	} 
}

public struct ComponentRecord(int id, SparseSetBase data) {
	public int Id = id;
	public SparseSetBase Data = data;
}