using Core.DataStructures;

namespace Core.ECS;

public static class Component<T> where T : struct {
	public static List<int> ID = new List<int>();
	internal static List<SparseSet<T>> internalSet = new List<SparseSet<T>>();
	public static ref int[] Sparse(int worldID) => ref internalSet[worldID].Sparse;
	public static ref int[] Dense(int worldID) => ref internalSet[worldID].Dense;
	public static ref T[] Data(int worldID) => ref internalSet[worldID].Data;

	public static void Add(int id, T data, int worldID) {
		internalSet[worldID].Add(id, data);
	}
	public static void Set(int id, T data, int worldID) {
		internalSet[worldID].Set(id, data);
	}
}