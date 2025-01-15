using Components;
using Core.ECS;
using Raylib_cs;
using System;
using System.Numerics;

public class Program {
	static Vector2 ScreenSize = new Vector2(1280, 720);
	static Camera2D camera = new Camera2D(ScreenSize/ 2, Vector2.Zero, 0, 1);
	static Texture2D magic;
	static float deltaTimeMultiplier;
	static float CameraSpeed = .5f;

	static float Randomised => Random.Shared.NextSingle() * 3;

	public static void Main(string[] args) {
		Raylib.InitWindow((int)ScreenSize.X, (int)ScreenSize.Y, "Wah");
		Raylib.SetExitKey(KeyboardKey.Null);

		magic = Raylib.LoadTexture("Assets/Magic.png"); ;

		World world = ECS.CreateWorld()
			.RegisterComponent<Position>()
			.RegisterComponent<Velocity>()
			.RegisterComponent<Renderable>()
			.Initialise();

		for (int i = 0; i < 50_000; i++) {
			world.GetEntity(i)
				.Set<Position>(new(0, 0))
				.Set<Velocity>(new Velocity(5, 5).RotatedBy(
					Random.Shared.Next(-4, 4) * Random.Shared.NextSingle()
				))
				.Set<Renderable>(new(magic, new(Random.Shared.Next(255), Random.Shared.Next(255), Random.Shared.Next(255))));
		}

		Raylib.SetTargetFPS(60);

		ComponentData<Position> position = world.GetComponent<Position>();
		ComponentData<Velocity> velocity = world.GetComponent<Velocity>();
		ComponentData<Renderable> renderable = world.GetComponent<Renderable>();
		
		while (!Raylib.IsKeyPressed(KeyboardKey.Space)) {
			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.Black);
			Raylib.DrawFPS(20, 20);
			Raylib.EndDrawing();
		}

		while (!Raylib.WindowShouldClose()) {
			deltaTimeMultiplier = Raylib.GetFrameTime() * 60;
			foreach (var i in Query<Position, Velocity>.Execute(world)) {
				position.DataStore.Data[i].Value += velocity.DataStore.Data[i].Value * deltaTimeMultiplier;
				var pos = position.DataStore.Data[i].Value;
				var vel = velocity.DataStore.Data[i].Value;
				if (pos.X < camera.Target.X - ScreenSize.X / 2 && vel.X < 0) velocity.DataStore.Data[i].Value *= -1;
				if (pos.X > camera.Target.X + ScreenSize.X / 2 && vel.X > 0) velocity.DataStore.Data[i].Value *= -1;
				if (pos.Y < camera.Target.Y - ScreenSize.Y / 2 && vel.Y < 0) velocity.DataStore.Data[i].Value *= -1;
				if (pos.Y > camera.Target.Y + ScreenSize.Y / 2 && vel.Y > 0) velocity.DataStore.Data[i].Value *= -1;
			}

			HandleMovement();
			Render(camera, ECS.ActiveWorld, position.DataStore.Data, renderable.DataStore.Data);

			position.Dirty = false;
		}
	}
	static void Render(Camera2D camera, World world, Position[] position, Renderable[] renderable) {
		Raylib.BeginDrawing();
		Raylib.ClearBackground(Color.Black);

		Raylib.BeginMode2D(camera);
			foreach (var i in Query<Position, Renderable>.Execute(world)) {
				if (!IsOnScreen(position[i].Value, camera)) continue;

				Raylib.DrawTexture(
					renderable[i].Texture, 
					(int)(position[i].Value.X), (int)(position[i].Value.Y), 
					renderable[i].Color
				);
			}
		Raylib.EndMode2D();

		Raylib.DrawFPS(20, 20);
		Raylib.EndDrawing();
	}

	static void HandleMovement() {
		if (Raylib.IsKeyDown(KeyboardKey.A)) camera.Target.X -= CameraSpeed * deltaTimeMultiplier;
		if (Raylib.IsKeyDown(KeyboardKey.D)) camera.Target.X += CameraSpeed * deltaTimeMultiplier;
		if (Raylib.IsKeyDown(KeyboardKey.S)) camera.Target.Y += CameraSpeed * deltaTimeMultiplier;
		if (Raylib.IsKeyDown(KeyboardKey.W)) camera.Target.Y -= CameraSpeed * deltaTimeMultiplier;
		if (Raylib.IsKeyDown(KeyboardKey.Z)) camera.Zoom = 2;
		else camera.Zoom = 1;
	}

	static bool IsOnScreen(Vector2 point, Camera2D camera) {
		return point.X > camera.Target.X - ScreenSize.X / 2 && point.X < camera.Target.X + ScreenSize.X / 2
			&& point.Y > camera.Target.Y - ScreenSize.Y / 2 && point.Y < camera.Target.Y + ScreenSize.Y / 2;
	}
}