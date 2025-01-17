using Core.DataStructures;
using System;
using System.Collections.Generic;

namespace Core.ECS;

public class World(int worldID) {
	public static event Action<World>? OnInitialise;

	public event Action<World>? OnActivate;

	public int ID = worldID;

	public BitSet[] Entities = [];

	int componentCount = 0;

	internal List<Action<World>> UpdateSystems = [];
	
	internal List<string> UpdateSystemNames = [];

	internal List<Action<World>> RenderSystems = [];

	internal List<string> RenderSystemNames = [];

	public void Activate() {
		OnActivate?.Invoke(this);
	}

	public World Initialise() {
		OnInitialise?.Invoke(this);
		Entities = new BitSet[128];
		for (int i = 0; i < 128; i++) {
			Entities[i] = new BitSet(componentCount);
		}
		return this;
	}

	public Entity GetEntity(int id) {
		EnsureCapacity(id + 1);
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

	#region Component handling
	public World RegisterComponent<T>() where T : struct {
		Component<T>.AddToWorld(ID, componentCount++);
		return this;
	}
	public World RegisterSingletonComponent<T>(T value) {
		SingletonComponent<T>.AddToWorld(ID, value);
		return this;
	}

	public ref ComponentData<T> GetComponent<T>() where T : struct {
		return ref Component<T>.Data[ID];
	}

	public ref T GetSingletonComponent<T>() {
		return ref SingletonComponent<T>.Data[ID];
	}
	#endregion

	#region System handling
	public World RegisterSystem(SystemType type, Action<World> system, string name, string[]? after = null) {
		if (type == SystemType.Update) RegisterSystem_Internal(ref UpdateSystems, ref UpdateSystemNames, system, name, after);
		if (type == SystemType.Render) RegisterSystem_Internal(ref RenderSystems, ref RenderSystemNames, system, name, after);
		return this;
	}

	internal void RegisterSystem_Internal(ref List<Action<World>> typeSystems, ref List<string> typeSystemNames, Action<World> system, string name, string[]? after = null) {
		if (after == null) {
			typeSystems.Add(system);
			typeSystemNames.Add(name);
			return;
		}

		int index = 0;
		foreach (string systemName in after) {
			index = Math.Max(index, typeSystemNames.IndexOf(systemName));
		}
		typeSystemNames.Insert(index + 1, name);
		typeSystems.Insert(index + 1, system);
	}

	public void Update() {
		for (int i = 0; i < UpdateSystems.Count; i++) {
			try { UpdateSystems[i].Invoke(this); }
			catch (Exception ex) { Console.WriteLine($"Exception {ex} caught in system {UpdateSystemNames[i]}"); }
		}
	}

	public void Render() {
		for (int i = 0; i < RenderSystems.Count; i++) {
			try { RenderSystems[i].Invoke(this); }
			catch (Exception ex) { Console.WriteLine($"Exception {ex} caught in system {RenderSystemNames[i]}"); }
		}
	}
	#endregion
}

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

public enum SystemType : byte {
	Update,
	Render
}
