using Core.DataStructures;
using System;

namespace Core.ECS;

public static class Component<T> where T : struct {
	internal static (SparseSet<T> DataStore, int Offset)[] Data = [];

	internal static void AddToWorld(int worldID, int offset) {
		if (Data.Length <= worldID) {
			Array.Resize(ref Data, Math.Max(worldID + 1, Data.Length * 2));
		}

		Data[worldID] = new(new SparseSet<T>(128), offset);
	}
}