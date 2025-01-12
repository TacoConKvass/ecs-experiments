using Core.ECS;
using Raylib_cs;
using System;
using System.Numerics;

World world = ECS.CreateWorld()
	.RegisterComponent<Position>()
	.RegisterComponent<Velocity>()
	.Initialise();

for (int i = 0; i < 50_000; i++) {
	world.GetEntity(i)
		.Set<Velocity>(new Velocity(Random.Shared.Next(-80, 80), Random.Shared.Next(-80, 80)).Normalise())
		.Set<Position>(new(500, 400));
}

Raylib.InitWindow(1280, 720, "Wah");
Raylib.SetExitKey(KeyboardKey.Null);

while (!Raylib.WindowShouldClose()) {
	var pos = world.GetComponent<Position>();
	var vel = world.GetComponent<Velocity>();
	if (Raylib.IsKeyDown(KeyboardKey.Space)) {
		for (int i = 0; i < 50_000; i++) {
			ref var p = ref pos.Data[i];
			p.Data += vel.Data[i].Data * 0.1f;
		}
	}
	
	Raylib.BeginDrawing();
	Raylib.ClearBackground(Color.Black);
	foreach (var test in new Query<Position, Velocity>(world).Execute().MoveNext())
		Raylib.DrawCircle((int)pos.Data[i].Data.X, (int)pos.Data[i].Data.Y, 2, Color.Beige);
	Raylib.DrawFPS(20, 20);

	Raylib.EndDrawing();
}

public struct Position(float x, float y) {
	public Vector2 Data = new Vector2(x, y);
}

public struct Velocity(float x, float y) {
	public Vector2 Data = new Vector2(x, y);

	public Velocity RotatedBy(float angle) {
		Data = new Vector2(Data.X * MathF.Cos(angle), Data.Y * MathF.Cos(angle));
		return this;
	}
	public Velocity Normalise() {
		Data = new Vector2(Data.X / Data.Length(), Data.Y / Data.Length());
		return this;
	}
}