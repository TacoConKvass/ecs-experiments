using Demo.Utils;
using System;
using System.Collections;
using System.Linq;

Console.Clear();

var world = new ECS.World(32);
Expect.Success("World has no components", world.ComponentCount == 0);

world.AddComponent<Vector2>();
Expect.Success("World has 1 component", world.ComponentCount == 1);

var component = world.GetComponent<Vector2>();
Expect.Success("Component with id 0 is of type Vector2", component.Id == 0);

var new_entity = new ECS.Entity(world);
new_entity.Set(Vector2.Zero);
Expect.Success("BitArray values matches between Entity instance and world.Entities", new_entity.componentFlags == world.Entities.componentFlags[0]);

var entity_0 = world.Entities[0];
entity_0.Set(new Vector2(1, 1));
Expect.Success("new_entity is the same as entity_0 if they share the same id", new_entity.GetSafe<Vector2>() == entity_0.Get<Vector2>());

var entity_1 = world.Entities[1];
Expect.Failure("new_entity is not the same as entity_1 as they do not share the same id", new_entity.componentFlags.BinaryValues() == entity_1.componentFlags.BinaryValues());

world.Entities[1] = entity_0;
Expect.Success("Data of world.Entities[0] copied to world.Entities[1]", new_entity.Get<Vector2>() == entity_0.Get<Vector2>());

entity_0.Remove<Vector2>();
Expect.Success("Vector2 component removed from entity", entity_0.GetSafe<Vector2>() == null);
Expect.Success("entity_1 still has the Vector2 component", entity_1.Has<Vector2>());

world.AddComponent<int>();
var component_int = world.GetComponent<int>();
Expect.Success("Component of type int has id 1", component_int.Id == 1);

var vector2_mask = ECS.QueryCache.MakeMask<Vector2>(world);
var mask_10 = new BitArray([true, false]);
Expect.Success("Mask for Vector2 is equal to 10", vector2_mask.BinaryValues() == mask_10.BinaryValues());

var int_mask = ECS.QueryCache.MakeMask<int>(world);
var mask_01 = new BitArray([false, true]);
Expect.Success("Mask for int is equal to 01", int_mask.BinaryValues() == mask_01.BinaryValues());

var and_mask = ECS.QueryCache.MakeMask<ECS.And<Vector2, int>>(world);
var mask_11 = new BitArray([true, true]);
Expect.Success("Mask for And<Vector2, int> is equal to 11", and_mask.BinaryValues() == mask_11.BinaryValues());

entity_0.Set<int>(1);
entity_1.Set<int>(10);
world.Entities[2].Set<Vector2>(Vector2.Zero);

var entities_with_Vector2 = world.Query<Vector2>();
Expect.Failure("The id of the first entity with a Vector2 component is not 0", entities_with_Vector2[0] == 0); 



Expect.FinalStatus();

public struct Vector2(float x, float y) {
    public float X = x;
    public float Y = y;

    public static Vector2 Zero => new Vector2(0, 0);

    public static bool operator ==(Vector2 v1, Vector2 v2) => v1.X == v2.X && v1.Y == v2.Y;
    public static bool operator !=(Vector2 v1, Vector2 v2) => v1.X != v2.X || v1.Y != v2.Y;
}
