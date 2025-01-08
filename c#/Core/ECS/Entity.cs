namespace Core.ECS;

public struct Entity {
	internal static int ID = 0;
	internal static ECS World;
	public bool Has<T>() where T : struct {
		return World.EntityHas<T>(ID);
	}
	public Entity Set<T>(T data) where T : struct {
		World.Set(ID, data);
		return this;
	}
}