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

	internal Stack<int> freeEntityIds;

	public int Id;
	public int ComponentCount;
	public EntityMap Entities;

	public World(int initial_size = 256) {
		Id = ++lastWorldId;
		ComponentCount = 0;
		freeEntityIds = new Stack<int>(Enumerable.Range(0, initial_size).Reverse());

		Entities = new EntityMap() {
			componentFlags = [],
			world = this,
		};

		Array.Resize(ref Entities.componentFlags, initial_size);
		Array.Fill(Entities.componentFlags, new BitArray(ComponentCount));
	}

	public void AddComponent<T>() where T : struct {
		ComponentStorage<T>.AddTo(this);
		ComponentCount++;
		for (int i = 0; i < Entities.componentFlags.Length; i++) {
			Entities.componentFlags[i].Length = ComponentCount;
		}
	}

	public ComponentStorage<T>.ComponentRecord GetComponent<T>() where T : struct => ComponentStorage<T>.GetFrom(this);

	internal int GetFreeId() {
		return freeEntityIds.Pop();
	}
}

public ref struct Entity {
	public int Id { get; init; }
	public ref BitArray ComponentFlags;

	internal World world;

	public Entity(World e_world, int? id = null) {
		Id = id ?? e_world.GetFreeId();
		ComponentFlags = ref e_world.Entities.componentFlags[Id];
		world = e_world;
	}

	public bool Has<T>() where T : struct {
		int index = ComponentStorage<T>.GetFrom(world).Id;
		return ComponentFlags[index];
	}

	public T? Get<T>() where T : struct {
		return ComponentStorage<T>.GetFrom(world).Data.TryGet(Id);
	}

	public bool TryGet<T>(out T? data) where T : struct {
		data = Get<T>();

		return data != null;
	}

	public void Set<T>(T data) where T : struct {
		ComponentStorage<T>.ComponentRecord component = ComponentStorage<T>.GetFrom(world);
		ComponentFlags[component.Id] = true;
		component.Data.Set(Id, data);
	}

	public void Remove<T>() where T : struct {
		ComponentStorage<T>.ComponentRecord component = ComponentStorage<T>.GetFrom(world);
		ComponentFlags[component.Id] = false;
		component.Data.Delete(Id);
	}
}

public static class ComponentStorage<T> where T : struct {
	public ref struct ComponentRecord(int id, ref SparseSet<T> data) {
		public int Id = id;
		public ref SparseSet<T> Data = ref data;
	}

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
		return new ComponentRecord(id[world.Id], ref components[world.Id]);
	} 
}