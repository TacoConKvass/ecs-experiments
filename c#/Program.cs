using Core.ECS;

ECS.RegisterComponent<Position>();
ECS.RegisterComponent<Velocity>();
ECS world = ECS.InitialiseWorld();

world.GetEntity(1)
	.Set<Position>(new(1, 0))
	.Set<Velocity>(new(1, 0));

world.QueryByMask([1]);

Console.WriteLine(world.entityFlags[1, 0]);

public struct Position(int x, int y) {
	public int X = x;
	public int Y = y;
}

public struct Velocity(int x, int y) {
	public int X = x;
	public int Y = y;
}