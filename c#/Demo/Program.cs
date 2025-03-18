using Demo.Utils;
using System;
using System.Collections;
using System.Linq;

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
Assert.Failure("new_entity is not the same as entity_from_map_1 as they do not share the same id", new_entity.componentFlags.BinaryValues() == entity_from_map_1.componentFlags.BinaryValues());

world.Entities[1] = entity_from_map;
Assert.Success("Data of world.Entities[0] copied to world.Entities[1]", new_entity.Get<Vector2>()! == entity_from_map.Get<Vector2>()!);

entity_from_map.Remove<Vector2>();
Assert.Success("Vector2 component removed from entity_from_map", entity_from_map.Get<Vector2>() == null);

world.AddComponent<int>();
var component_int = world.GetComponent<int>();
Assert.Success("Component of type int has id 1", component_int.Id == 1);

var vector2_mask = ECS.QueryCache.MakeMask<Vector2>(world);
var mask_10 = new BitArray([true, false]);
Assert.Success("Mask for Vector2 is equal to 10", vector2_mask.BinaryValues() == mask_10.BinaryValues());

var int_mask = ECS.QueryCache.MakeMask<int>(world);
var mask_01 = new BitArray([false, true]);
Assert.Success("Mask for int is equal to 01", int_mask.BinaryValues() == mask_01.BinaryValues());

var and_mask = ECS.QueryCache.MakeMask<ECS.And<Vector2, int>>(world);
var mask_11 = new BitArray([true, true]);
Assert.Success("Mask for And<Vector2, int> is equal to 11", and_mask.BinaryValues() == mask_11.BinaryValues());

var entities_with_Vector2 = world.Query<Vector2>();
entity_from_map_1.Set(Vector2.Zero);
Console.WriteLine(string.Join(" ", world.Entities.componentFlags.Select(x => x.BinaryValues())));
Assert.Failure("The id of the first entity with a Vector2 component is not 0", true); 

Assert.FinalStatus();

public struct Vector2(float x, float y) {
	public float X = x;
	public float Y = y;

	public static Vector2 Zero => new Vector2(0, 0);

	public static bool operator ==(Vector2 v1, Vector2 v2) => v1.X == v2.X && v1.Y == v2.Y;
	public static bool operator !=(Vector2 v1, Vector2 v2) => v1.X != v2.X || v1.Y != v2.Y;
}