using System;
using Core.ECS;

World world = ECS.CreateWorld()
	.RegisterComponent<Position>()
	.RegisterComponent<Velocity>()
	.Initialise();

Console.WriteLine(world.Entities[0].Flags[0]);
Console.WriteLine(world.Entities[5].Flags[0]);

world.GetEntity(0).Set<Position>(new(1, 0));
world.GetEntity(5).Set<Velocity>(new(1, 0));
world.GetEntity(15).Set<Velocity>(new(1, 0)).Set<Position>(new(1, 0));

Console.WriteLine(world.Entities[0].Flags[0]);
Console.WriteLine(world.Entities[5].Flags[0]);
Console.WriteLine(world.Entities[15].Flags[0]);

public struct Position(int x, int y) {
	public int X = x;
	public int Y = y;
}

public struct Velocity(int x, int y) {
	public int X = x;
	public int Y = y;
}