using Core.DataStructures;
using System;

namespace Core.ECS;

public static class Component<T> where T : struct {
	internal static ComponentData<T>[] Data = [];

	internal static void AddToWorld(int worldID, int offset) {
		if (Data.Length <= worldID) {
			Array.Resize(ref Data, Math.Max(worldID + 1, Data.Length * 2));
		}

		Data[worldID] = new ComponentData<T>(128, offset);
	}
}

public static class SingletonComponent<T> {
	internal static T[] Data = [];

	internal static void AddToWorld(int worldID, T value) {
		if (Data.Length <= worldID) {
			Array.Resize(ref Data, Math.Max(worldID + 1, Data.Length * 2));
		}

		Data[worldID] = value;
	}
}

public struct ComponentData<T>(int size, int offset) where T : struct {
	public SparseSet<T> DataStore = new SparseSet<T>(size);
	public int Offset = offset;
	public bool Dirty = true;
}