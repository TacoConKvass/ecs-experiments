using Core.ECS;

ECS.RegisterComponent<Position>();
ECS.RegisterComponent<Velocity>();
ECS world = ECS.InitialiseWorld();

world.AddToEntity(0, new Position(1, 1));
world.AddToEntity(1, new Velocity(1, 1));

Console.WriteLine(world.EntityHas<Position>(0));

public struct Position(int x, int y) {
	public int X = x;
	public int Y = y;
}

public struct Velocity(int x, int y) {
	public int X = x;
	public int Y = y;
}