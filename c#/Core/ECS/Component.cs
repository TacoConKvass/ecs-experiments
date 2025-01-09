using Core.DataStructures;

namespace Core.ECS;

public static class Component<T> where T : struct {
	public static List<int> ID = new List<int>();
	
	internal static List<SparseSet<T>> internalSet = new List<SparseSet<T>>();
	
	public static int[] Sparse(int worldID) => internalSet[worldID].Sparse;
	
	public static int[] Dense(int worldID) => internalSet[worldID].Dense;
	
	public static T[] Data(int worldID) => internalSet[worldID].Data;

	public static void Add(int id, T data, int worldID) {
		internalSet[worldID].Add(id, data);
	}

	public static void Set(int id, T data, int worldID) {
		internalSet[worldID].Set(id, data);
	}
}