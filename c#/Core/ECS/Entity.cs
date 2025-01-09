namespace Core.ECS;

public struct Entity(int id, byte worldID) {

	public int ID = id;
	internal byte WorldID = worldID;

	public bool Has<T>() where T : struct {
		return ECS.Worlds[WorldID].EntityHas<T>(ID);
	}

	public Entity Set<T>(T data) where T : struct {
		ECS.Worlds[WorldID].Set(ID, data);
		return this;
	}
}