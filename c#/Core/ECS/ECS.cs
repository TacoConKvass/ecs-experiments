using Core.DataStructures;

namespace Core.ECS;

public static class ECS {

	private static int componentCount = 0;

	private static byte worldCount = 0;

	internal static List<World> Worlds = new List<World>();

	public static World InitialiseWorld() {
		World wld = new World(componentCount, worldCount);
		Worlds.Add(wld);
		return wld;
	}

	public static void ResetComnponentRegister() {
		componentCount = 0;
	}

	public static void RegisterComponent<T>() where T : struct {
		if (Component<T>.ID.Count > worldCount) return;
		Component<T>.ID.Add(componentCount++);
		Component<T>.internalSet.Add(new SparseSet<T>(128));
	}
}
