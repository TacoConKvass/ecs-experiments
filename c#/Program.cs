using Core.ECS;

ECS.RegisterComponent<Position>();
ECS.RegisterComponent<Velocity>();
World world = ECS.InitialiseWorld();

world.GetEntity(0).Set<Position>(new(1, 0)).Set<Velocity>(new(1, 0));
world.GetEntity(1).Set<Velocity>(new(1, 0));
world.GetEntity(2).Set<Position>(new(1, 0));

Console.WriteLine(string.Join(", ", world.entityFlags[0].Flags));
Console.WriteLine(string.Join(", ", world.entityFlags[1].Flags));
Console.WriteLine(string.Join(", ", world.entityFlags[2].Flags));

public struct Position(int x, int y) {
	public int X = x;
	public int Y = y;
}

public struct Velocity(int x, int y) {
	public int X = x;
	public int Y = y;
}