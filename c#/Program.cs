﻿using Core.ECS;
using Components;
using Systems;
using Raylib_cs;
using System;
using System.Numerics;

public class Program {
	static readonly int entityCount = 100_000;

	public static Vector2 ScreenSize = new Vector2(1280, 720);
	public static Texture2D magic;
	public static float deltaTimeMultiplier;
	public static float CameraSpeed = 5f;

	public static World Loading = ECS.CreateWorld()
		.RegisterSystem(LoadingScene.HandleInput, "WaitForInput");

	public static World Demo = ECS.CreateWorld()
		.RegisterComponent<Position>()
		.RegisterComponent<Velocity>()
		.RegisterComponent<Renderable>()
		.RegisterSingletonComponent<Camera2D>(new(ScreenSize / 2, Vector2.Zero, 0, 1))
		.RegisterSystem(DemoSystems.MoveSquares, "MoveSquares")
		.RegisterSystem(DemoSystems.HandleInput, "InputHandling")
		.RegisterSystem(DemoSystems.Render, "Rendering")
		.Initialise();

	public static void Main(string[] args) {
		Raylib.InitWindow((int)ScreenSize.X, (int)ScreenSize.Y, "Wah");
		Raylib.SetExitKey(KeyboardKey.Null);

		magic = Raylib.LoadTexture("Assets/Magic.png");

		for (int i = 0; i < entityCount; i++) {
			Demo.GetEntity(i)
				.Set<Position>(new(0, 0))
				.Set<Velocity>(new Velocity(Random.Shared.NextSingle() * 4, Random.Shared.NextSingle() * 4).RotatedBy(
					Random.Shared.Next(-6, 6) * Random.Shared.NextSingle() * Random.Shared.NextSingle()
				))
				.Set<Renderable>(new(magic, new(Random.Shared.Next(255), Random.Shared.Next(255), Random.Shared.Next(255))));
		}

		ECS.SetActiveWorld(Loading);

		Raylib.SetTargetFPS(60);

		while (!Raylib.WindowShouldClose()) {
			deltaTimeMultiplier = Raylib.GetFrameTime() * 60;

			ECS.ActiveWorld.InvokeSystems();
		}

		Raylib.CloseWindow();
	}

	public static bool IsOnScreen(Vector2 point, Camera2D camera) {
		return point.X > camera.Target.X - ScreenSize.X / 2 / camera.Zoom && point.X < camera.Target.X + ScreenSize.X / 2 / camera.Zoom
			&& point.Y > camera.Target.Y - ScreenSize.Y / 2 / camera.Zoom && point.Y < camera.Target.Y + ScreenSize.Y / 2 / camera.Zoom;
	}
}