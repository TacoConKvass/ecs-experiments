using Core.ECS;
using Raylib_cs;
using System;
using System.Numerics;

World world = ECS.CreateWorld()
	.RegisterComponent<Position>()
	.RegisterComponent<Velocity>()
	.RegisterComponent<Color>()
	.Initialise();

for (int i = 0; i < 1_000_000; i++) {
	world.GetEntity(i)
		.Set<Position>(new (0, 0))
		.Set<Velocity>(new(Random.Shared.NextSingle(), Random.Shared.NextSingle()))
		.Set<Color>(new (Random.Shared.Next(255), Random.Shared.Next(255), Random.Shared.Next(255)));
}

Vector2 ViewportPosition = Vector2.Zero;
Vector2 ScreenSize = new Vector2(1280, 720);

Raylib.InitWindow((int)ScreenSize.X, (int)ScreenSize.Y, "Wah");
Raylib.SetExitKey(KeyboardKey.Null);

// Raylib.SetTargetFPS(60);

ref var pos = ref world.GetComponent<Position>();
var vel = world.GetComponent<Velocity>();
var ren = world.GetComponent<Color>().DataStore.Data;

ref var posi = ref pos.DataStore.Data;

var magic = Raylib.LoadTexture("Assets/Magic.png");
float deltaTime = 0;

while (!Raylib.IsKeyPressed(KeyboardKey.Space)) {
	Raylib.BeginDrawing();
	Raylib.ClearBackground(Color.Black); 
	Raylib.DrawFPS(20, 20);
	Raylib.EndDrawing();
}

while (!Raylib.WindowShouldClose()) {
	Query<Position>.Execute(world);
	foreach (var i in Query<Position>.Execute(world)) {
		pos.DataStore.Data[i].Value += vel.DataStore.Data[i].Value * deltaTime;
	}

	if (Raylib.IsKeyDown(KeyboardKey.A)) {
		ViewportPosition.X++;
	}
	if (Raylib.IsKeyDown(KeyboardKey.D)) {
		ViewportPosition.X--;
	}
	if (Raylib.IsKeyDown(KeyboardKey.S)) {
		ViewportPosition.Y--;
	}
	if (Raylib.IsKeyDown(KeyboardKey.W)) {
		ViewportPosition.Y++;
	}

	Raylib.BeginDrawing();
	Raylib.ClearBackground(Color.Black);
	int rendered = 0;
	
	foreach (var i in Query<Position, Color>.Execute(world)) {
		if (!IsOnScreen(posi[i].Value)) continue;

		Raylib.DrawTexture(magic, (int)(posi[i].Value.X + ViewportPosition.X), (int)(posi[i].Value.Y + ViewportPosition.Y), ren[i]);
		rendered++;
	}

	Raylib.DrawFPS(20, 20);
	Raylib.DrawText(rendered.ToString(), 20, 700, 20, Color.Red);

	Raylib.EndDrawing();
	deltaTime = Raylib.GetFrameTime() / 0.0166667f;
	pos.Dirty = false;
}

bool IsOnScreen(Vector2 point) {
	return point.X > -ViewportPosition.X && point.X < ScreenSize.X - ViewportPosition.X
		&& point.Y > -ViewportPosition.Y && point.Y < ScreenSize.Y - ViewportPosition.Y;
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