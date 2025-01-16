using Core.DataStructures;
using System;
using System.Collections.Generic;

namespace Core.ECS;

public class World(int worldID) {
	static event Action<World> OnInitialise;

	public int ID = worldID;

	public BitSet[] Entities = [];

	int componentCount = 0;

	internal List<Action<World>> Systems = [];
	
	internal List<string> SystemNames = [];

	internal List<Action<World>> RenderSystems = [];

	public World Initialise() {
		OnInitialise?.Invoke(this);
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

	#region Component handling
	public World RegisterComponent<T>() where T : struct
	{
		Component<T>.AddToWorld(ID, componentCount++);
		return this;
	}
	public World RegisterSingletonComponent<T>(T value)
	{
		SingletonComponent<T>.AddToWorld(ID, value);
		return this;
	}

	public ref ComponentData<T> GetComponent<T>() where T : struct
	{
		return ref Component<T>.Data[ID];
	}

	public ref T GetSingletonComponent<T>()
	{
		return ref SingletonComponent<T>.Data[ID];
	}
	#endregion

	#region System handling
	public World RegisterSystem(Action<World> system, string name, string[]? after = null)
	{
		if (after == null)
		{
			Systems.Add(system);
			SystemNames.Add(name);
			return this;
		}

		int index = 0;
		foreach (string systemName in after)
		{
			index = Math.Max(index, SystemNames.IndexOf(systemName));
		}
		SystemNames.Insert(index, name);
		Systems.Insert(index, system);
		return this;
	}

	public void InvokeSystems()
	{
		for (int i = 0; i < Systems.Count; i++)
		{
			try { Systems[i].Invoke(this); }
			catch (Exception ex) { Console.WriteLine($"Exception {ex} caught in system {SystemNames[i]}"); }
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