using Core.ECS;
using Raylib_cs;
using System;
using System.Linq;
using System.Numerics;

World world = ECS.CreateWorld()
	.RegisterComponent<Position>()
	.RegisterComponent<Velocity>()
	.Initialise();

for (int i = 0; i < 1_000_000; i++) {
	world.GetEntity(i)
		.Set<Position>(new(Random.Shared.Next(1280), Random.Shared.Next(720)));
}

Raylib.InitWindow(1280, 720, "Wah");
Raylib.SetExitKey(KeyboardKey.Null);

while (!Raylib.WindowShouldClose()) {
	var pos = world.GetComponent<Position>();
	var vel = world.GetComponent<Velocity>();

	Raylib.BeginDrawing();
	Raylib.ClearBackground(Color.Black);
	foreach (var test in Query<Position>.Execute(world))
		Raylib.DrawLine((int)test.Value.X, (int)test.Value.Y, (int)test.Value.X + 1, (int)test.Value.Y + 1, Color.Beige);
	Raylib.DrawFPS(20, 20);

	Raylib.EndDrawing();
}

public struct Position(float x, float y) {
	public Vector2 Value = new Vector2(x, y);
}

public struct Velocity(float x, float y) {
	public Vector2 Value = new Vector2(x, y);

	public Velocity RotatedBy(float angle) {
		Value = new Vector2(Value.X * MathF.Cos(angle), Value.Y * MathF.Sin(angle));
		return this;
	}
	public Velocity Normalise() {
		Value = new Vector2(Value.X / Value.Length(), Value.Y / Value.Length());
		return this;
	}
}