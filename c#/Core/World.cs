using ECS.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ECS;

public class World {
	public struct EntityMap {
		internal BitArray[] componentFlags;
		internal World world;

		public Entity this[int index] {
			get => new Entity(world, index);
		} 
	}	

	internal static int lastWorldId = -1;

	internal int nextEntityId = 0;
	internal Stack<int> freeEntityIds;
	internal List<SparseSetBase> components = [];

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
		Array.Fill(Entities.componentFlags, new BitArray(ComponentCount));
	}

	public void AddComponent<T>() where T : struct {
		ComponentStorage<T>.AddTo(this);
		components.Add(GetComponent<T>().Data);
		ComponentCount++;
		for (int i = 0; i < Entities.componentFlags.Length; i++) {
			Entities.componentFlags[i].Length = ComponentCount;
		}
	}

	public ComponentRecord GetComponent<T>() where T : struct => ComponentStorage<T>.GetFrom(this);

	internal int GetFreeId() {
		return freeEntityIds.TryPop(out int id) ? id : nextEntityId++;
	}
}

public ref struct Entity {
	public readonly int Id;
	public ref BitArray ComponentFlags => ref world.Entities.componentFlags[Id];

	internal readonly World world;

	public Entity(World e_world, int? id = null) {
		Id = id ?? e_world.GetFreeId();
		world = e_world;
	}

	public bool Has<T>() where T : struct {
		int index = ComponentStorage<T>.GetFrom(world).Id;
		return ComponentFlags[index];
	}

	public T? Get<T>() where T : struct {
		return (T?)(ComponentStorage<T>.GetFrom(world).Data.TryGet(Id));
	}

	public void Set<T>(T data) where T : struct {
		ComponentRecord component = ComponentStorage<T>.GetFrom(world);
		ComponentFlags[component.Id] = true;
		component.Data.Set(Id, data);
	}

	public void Remove<T>() where T : struct {
		ComponentRecord component = ComponentStorage<T>.GetFrom(world);
		ComponentFlags[component.Id] = false;
		component.Data.Delete(Id);
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