using Core.ECS;

ECS.RegisterComponent<Position>();
ECS.RegisterComponent<Velocity>();
ECS.RegisterComponent<Velocity2>();
ECS world = ECS.InitialiseWorld();

Console.WriteLine($"{world.entityFlags[0][0]:b}");
world.AddToEntity(0, new Position(1, 1));
world.AddToEntity(1, new Velocity(1, 1));
Console.WriteLine($"{world.entityFlags[1][0]:b}");

public struct Position(int x, int y) {
	public int X = x;
	public int Y = y;
}

public struct Velocity(int x, int y) {
	public int X = x;
	public int Y = y;
}

public struct Velocity2(int x, int y) {
	public int X = x;
	public int Y = y;
}