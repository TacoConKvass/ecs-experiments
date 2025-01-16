using Core.ECS;
using Components;
using Raylib_cs;
using System;

namespace Systems;

public class DemoSystems {
	public static void MoveSquares(World world) {
		Camera2D camera = world.GetSingletonComponent<Camera2D>();
		Position[] position = world.GetComponent<Position>().DataStore.Data;
		Velocity[] velocity = world.GetComponent<Velocity>().DataStore.Data;
		foreach (var i in Query<Position, Velocity>.Execute(world)) {
			position[i].Value += velocity[i].Value * Program.deltaTimeMultiplier;
			if (position[i].Value.X < camera.Target.X - Program.ScreenSize.X / 2 && velocity[i].Value.X < 0) velocity[i].Value *= -1;
			if (position[i].Value.X > camera.Target.X + Program.ScreenSize.X / 2 && velocity[i].Value.X > 0) velocity[i].Value *= -1;
			if (position[i].Value.Y < camera.Target.Y - Program.ScreenSize.Y / 2 && velocity[i].Value.Y < 0) velocity[i].Value *= -1;
			if (position[i].Value.Y > camera.Target.Y + Program.ScreenSize.Y / 2 && velocity[i].Value.Y > 0) velocity[i].Value *= -1;
		}
	}

	public static void HandleInput(World world) {
		Camera2D camera = world.GetSingletonComponent<Camera2D>();
		if (Raylib.IsKeyDown(KeyboardKey.A)) camera.Target.X -= Program.CameraSpeed * Program.deltaTimeMultiplier;
		if (Raylib.IsKeyDown(KeyboardKey.D)) camera.Target.X += Program.CameraSpeed * Program.deltaTimeMultiplier;
		if (Raylib.IsKeyDown(KeyboardKey.S)) camera.Target.Y += Program.CameraSpeed * Program.deltaTimeMultiplier;
		if (Raylib.IsKeyDown(KeyboardKey.W)) camera.Target.Y -= Program.CameraSpeed * Program.deltaTimeMultiplier;
		if (Raylib.IsKeyDown(KeyboardKey.Z) && camera.Zoom < 2) camera.Zoom += .01f * Program.deltaTimeMultiplier;
		else if (camera.Zoom > 1) camera.Zoom = Math.Clamp(camera.Zoom - .05f * Program.deltaTimeMultiplier, 1, float.PositiveInfinity);
		if (Raylib.IsKeyPressed(KeyboardKey.F11)) Raylib.ToggleFullscreen();
		if (Raylib.IsKeyDown(KeyboardKey.X)) camera.Rotation += 1 * Program.deltaTimeMultiplier;
		else if (camera.Rotation > 0) camera.Rotation = Math.Clamp(camera.Rotation - 1 * Program.deltaTimeMultiplier, 0, float.PositiveInfinity);
		if (Raylib.IsKeyPressed(KeyboardKey.Space)) ECS.SetActiveWorld(Program.Loading);
	}

	public static void Render(World world) {
		Camera2D camera = world.GetSingletonComponent<Camera2D>();
		Position[] position = world.GetComponent<Position>().DataStore.Data;
		Renderable[] renderable = world.GetComponent<Renderable>().DataStore.Data;

		Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.Black);
		
			Raylib.BeginMode2D(camera);
				foreach (var i in Query<Position, Renderable>.Execute(world)) {
					if (!Program.IsOnScreen(position[i].Value, camera)) continue;

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
}