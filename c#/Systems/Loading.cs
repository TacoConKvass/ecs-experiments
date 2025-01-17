using Core.ECS;
using Raylib_cs;

namespace Systems;

public static class LoadingScene {
	static string[] dots = ["", ".", "..", "..."];
	static float frame = 0;

	public static void Update(World world) {
		if (frame > 12) {
			ECS.SetActiveWorld(Program.Demo);
			frame = 0;
			return;
		}

		if (Raylib.IsKeyPressed(KeyboardKey.F11)) Raylib.ToggleFullscreen();

		frame += 0.1f * Program.deltaTimeMultiplier;
	}

	public static void Render(World world) {
		Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.Black);

			Raylib.DrawText($"Loading {dots[(int)frame % dots.Length]}", 300, 300, 20, Color.White);

			Raylib.DrawFPS(20, 20);
		Raylib.EndDrawing();
	}
}