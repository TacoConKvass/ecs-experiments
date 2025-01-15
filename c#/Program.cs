using Components;
using Core.ECS;
using Raylib_cs;
using System;
using System.Numerics;

public class Program {
	static readonly int entityCount = 100_000;

	static Vector2 ScreenSize = new Vector2(1280, 720);
	static Camera2D camera = new Camera2D(ScreenSize/ 2, Vector2.Zero, 0, 1);
	static Texture2D magic;
	static float deltaTimeMultiplier;
	static float CameraSpeed = 5f;

	static World MainWorld = ECS.CreateWorld()
			.RegisterComponent<Position>()
			.RegisterComponent<Velocity>()
			.RegisterComponent<Renderable>()
			.Initialise();

	public static void Main(string[] args) {
		Raylib.InitWindow((int)ScreenSize.X, (int)ScreenSize.Y, "Wah");
		Raylib.SetExitKey(KeyboardKey.Null);

		magic = Raylib.LoadTexture("Assets/Magic.png");

		for (int i = 0; i < entityCount; i++) {
			MainWorld.GetEntity(i)
				.Set<Position>(new(0, 0))
				.Set<Velocity>(new Velocity(4, 4).RotatedBy(
					Random.Shared.Next(-6, 6) * Random.Shared.NextSingle() * Random.Shared.NextSingle()
				))
				.Set<Renderable>(new(magic, new(Random.Shared.Next(255), Random.Shared.Next(255), Random.Shared.Next(255))));
		}

		// Raylib.SetTargetFPS(60);

		ComponentData<Position> position = MainWorld.GetComponent<Position>();
		ComponentData<Velocity> velocity = MainWorld.GetComponent<Velocity>();
		ComponentData<Renderable> renderable = MainWorld.GetComponent<Renderable>();
		
		while (!Raylib.IsKeyPressed(KeyboardKey.Space)) {
			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.Black);
			Raylib.DrawFPS(20, 20);
			HandleInput();
			Raylib.EndDrawing();
		}

		while (!Raylib.WindowShouldClose()) {
			deltaTimeMultiplier = Raylib.GetFrameTime() * 60;

			MoveSquares(position.DataStore.Data, velocity.DataStore.Data);
			HandleInput();
			
			Render(camera, ECS.ActiveWorld, position.DataStore.Data, renderable.DataStore.Data);

			position.Dirty = false;
		}
	}

	static void MoveSquares(Position[] position, Velocity[] velocity) {
		foreach (var i in Query<Position, Velocity>.Execute(ECS.ActiveWorld)) {
			position[i].Value += velocity[i].Value * deltaTimeMultiplier;
			if (position[i].Value.X < camera.Target.X - ScreenSize.X / 2 && velocity[i].Value.X < 0) velocity[i].Value *= -1;
			if (position[i].Value.X > camera.Target.X + ScreenSize.X / 2 && velocity[i].Value.X > 0) velocity[i].Value *= -1;
			if (position[i].Value.Y < camera.Target.Y - ScreenSize.Y / 2 && velocity[i].Value.Y < 0) velocity[i].Value *= -1;
			if (position[i].Value.Y > camera.Target.Y + ScreenSize.Y / 2 && velocity[i].Value.Y > 0) velocity[i].Value *= -1;
		}
	}

	static void HandleInput() {
		if (Raylib.IsKeyDown(KeyboardKey.A)) camera.Target.X -= CameraSpeed * deltaTimeMultiplier;
		if (Raylib.IsKeyDown(KeyboardKey.D)) camera.Target.X += CameraSpeed * deltaTimeMultiplier;
		if (Raylib.IsKeyDown(KeyboardKey.S)) camera.Target.Y += CameraSpeed * deltaTimeMultiplier;
		if (Raylib.IsKeyDown(KeyboardKey.W)) camera.Target.Y -= CameraSpeed * deltaTimeMultiplier;
		if (Raylib.IsKeyDown(KeyboardKey.Z) && camera.Zoom < 2) camera.Zoom += .01f * deltaTimeMultiplier;
		else if (camera.Zoom > 1) camera.Zoom -= .05f * deltaTimeMultiplier;
		if (Raylib.IsKeyPressed(KeyboardKey.F11)) Raylib.ToggleFullscreen();
	}

	static void Render(Camera2D camera, World world, Position[] position, Renderable[] renderable) {
		Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.Black);

			Raylib.BeginMode2D(camera);
				foreach (var i in Query<Position, Renderable>.Execute(world)) {
					if (!IsOnScreen(position[i].Value, camera)) continue;

					Raylib.DrawTexture(
						renderable[i].Texture, 
						(int)(position[i].Value.X - renderable[i].Texture.Width / 2), (int)(position[i].Value.Y - renderable[i].Texture.Height / 2), 
						renderable[i].Color
					);
				}
			Raylib.EndMode2D();

			Raylib.DrawFPS(20, 20);
		Raylib.EndDrawing();
	}

	static bool IsOnScreen(Vector2 point, Camera2D camera) {
		return point.X > camera.Target.X - ScreenSize.X / 2 / camera.Zoom && point.X < camera.Target.X + ScreenSize.X / 2 / camera.Zoom
			&& point.Y > camera.Target.Y - ScreenSize.Y / 2 / camera.Zoom && point.Y < camera.Target.Y + ScreenSize.Y / 2 / camera.Zoom;
	}
}