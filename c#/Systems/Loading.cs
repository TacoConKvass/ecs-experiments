using Core.ECS;
using Raylib_cs;

namespace Systems;

public static class LoadingScene {
	static string[] dots = ["", ".", "..", "..."];
	static float frame = 0;

	public static void HandleInput(World world) {
		while (!Raylib.IsKeyPressed(KeyboardKey.Space)) {
			frame += 0.1f;

			Raylib.BeginDrawing();
				Raylib.ClearBackground(Color.Black);

				Raylib.DrawText($"Loading {dots[(int)frame % dots.Length]}", 300, 300, 20, Color.White);

				Raylib.DrawFPS(20, 20);
			Raylib.EndDrawing();
		}

		ECS.SetActiveWorld(Program.Demo);
	}
}