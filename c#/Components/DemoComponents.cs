using Raylib_cs;
using System.Numerics;
using System;

namespace Components;

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

public struct Renderable(Texture2D texture, Color color) {
	public Color Color = color;
	public Texture2D Texture = texture;
}