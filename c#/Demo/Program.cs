using Demo.Utils;
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
Assert.Success("BitArray values matches between Entity instance and world.Entities", new_entity.componentFlags == world.Entities.componentFlags[0]);

var entity_from_map = world.Entities[0];
entity_from_map.Set(new Vector2(1, 1));
Assert.Success("new_entity is the same as entity_from_map if they share the same id", new_entity.Get<Vector2>()! == entity_from_map.Get<Vector2>()!);

var entity_from_map_1 = world.Entities[1];
Assert.Failure("new_entity is not the same as entity_from_map_1 as they do not share the same id", new_entity.componentFlags == entity_from_map_1.componentFlags);

world.Entities[1] = entity_from_map;
Assert.Success("Data of world.Entities[0] copied to world.Entities[1]", new_entity.Get<Vector2>()! == entity_from_map.Get<Vector2>()!);

entity_from_map.Remove<Vector2>();
Assert.Success("Vector2 component removed from entity_from_map", entity_from_map.Get<Vector2>() == null);



Assert.FinalStatus();

public struct Vector2(float x, float y) {
	public float X = x;
	public float Y = y;

	public static Vector2 Zero => new Vector2(0, 0);

	public static bool operator ==(Vector2 v1, Vector2 v2) => v1.X == v2.X && v1.Y == v2.Y;
	public static bool operator !=(Vector2 v1, Vector2 v2) => v1.X != v2.X || v1.Y != v2.Y;
}