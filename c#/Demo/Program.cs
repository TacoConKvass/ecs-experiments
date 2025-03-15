using System;

Console.Clear();

var world = new ECS.World(32);
Assert.Success("World has no components", world.ComponentCount == 0);

world.AddComponent<Vector2>();
Assert.Success("World has 1 component", world.ComponentCount == 1);

var component = world.GetComponent<Vector2>();
Assert.Success("Component with id 0 is of type Vector2", component.Id == 0);

var new_entity = new ECS.Entity(world);
new_entity.Set(Vector2.Zero);
Assert.Success("BitArray values matches between Entity instance and world.Entities", new_entity.ComponentFlags == world.Entities.componentFlags[0]);

var entity_from_map = world.Entities[0];
entity_from_map.Set(new Vector2(1, 1));
Assert.Success("new_entity is the same as entity_from_map if they share the same id", new_entity.Get<Vector2>()! == entity_from_map.Get<Vector2>()!);

entity_from_map.Remove<Vector2>();
Assert.Success("Vector2 component removed from entity_from_map", entity_from_map.Get<Vector2>() == null);

public struct Vector2(float x, float y) {
	public float X = x;
	public float Y = y;

	public static Vector2 Zero => new Vector2(0, 0);

	public static bool operator ==(Vector2 v1, Vector2 v2) => v1.X == v2.X && v1.Y == v2.Y;
	public static bool operator !=(Vector2 v1, Vector2 v2) => v1.X != v2.X || v1.Y != v2.Y;
}

public static class Assert {
	public static int TestNumber = 0;
	public static void Success(string test, bool expected) {
		Console.Write($"[Test {++TestNumber} - ");
		Console.ForegroundColor = expected ? ConsoleColor.Green : ConsoleColor.Red;
		Console.Write($"{(expected ? "Passed \u2713" : "Failed \u274C")}");
		Console.ResetColor();
		Console.Write($"]: {test}\n");
	}
}