using Core.DataStructures;
using System.Collections.Generic;

namespace Core.ECS;

public class Query<T>(World world) where T : struct {
	World World = world;
	(SparseSet<T> Data, int ID) Component = Component<T>.Data[world.ID];

	public IEnumerable<T> Execute() {
		for (int i = 0; i < World.Entities.Length; i++) {
			BitSet entity = World.Entities[i];
			if (!entity.Has(Component.ID)) continue;
			yield return Component.Data.Get(i);
		}
	}
}

public class Query<T1, T2>
		where T1 : struct 
		where T2 : struct {
	public static IEnumerable<(T1 Component1, T2 Component2)> Execute(World world) {
		(SparseSet<T1> Data, int ID) Component1 = Component<T1>.Data[world.ID];
		(SparseSet<T2> Data, int ID) Component2 = Component<T2>.Data[world.ID];
		for (int i = 0; i < world.Entities.Length; i++) {
			BitSet entity = world.Entities[i];
			if (!entity.Has(Component1.ID) || !entity.Has(Component2.ID)) continue;
			yield return (Component1.Data.Get(i), Component2.Data.Get(i));
		}
	}
}

public class Query<T1, T2, T3>(World world)
		where T1 : struct
		where T2 : struct
		where T3 : struct {
	World World = world;
	(SparseSet<T1> Data, int ID) Component1 = Component<T1>.Data[world.ID];
	(SparseSet<T2> Data, int ID) Component2 = Component<T2>.Data[world.ID];
	(SparseSet<T3> Data, int ID) Component3 = Component<T3>.Data[world.ID];

	public IEnumerable<(T1, T2, T3)> Execute() {
		for (int i = 0; i < World.Entities.Length; i++) {
			BitSet entity = World.Entities[i];
			if (!entity.Has(Component1.ID) || !entity.Has(Component2.ID) || !entity.Has(Component3.ID)) continue;
			yield return (Component1.Data.Get(i), Component2.Data.Get(i), Component3.Data.Get(i));
		}
	}
}