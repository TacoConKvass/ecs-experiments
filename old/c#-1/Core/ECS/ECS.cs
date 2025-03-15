namespace Core.ECS;

public static class ECS {
	public static World ActiveWorld;

	static int WorldCount = 0;

	public static World SetActiveWorld(World world) {
		ActiveWorld = world;
		ActiveWorld.Activate();
		return ActiveWorld;
	}

	public static World CreateWorld() {
		return new World(WorldCount++);
	}
}