using Core.DataStructures;
using System.Collections.Generic;

namespace Core.ECS;

public class Query<T>(World world) : IEnumerator<T> where T : struct {
	World World = world;
	(SparseSet<T> Data, int ID) Component = Component<T>.Data[world.ID];

	public IEnumerator<T> Execute() {
		for (int i = 0; i < World.Entities.Length; i++) {
			BitSet entity = World.Entities[i];
			if (!entity.Has(Component.ID)) continue;
			yield return Component.Data.Get(i);
		}
	}
}

public class Query<T1, T2>(World world) 
		where T1 : struct
		where T2 : struct {
	World World = world;
	(SparseSet<T1> Data, int ID) Component1 = Component<T1>.Data[world.ID];
	(SparseSet<T2> Data, int ID) Component2 = Component<T2>.Data[world.ID];

	public IEnumerator<(T1, T2)> Execute() {
		for (int i = 0; i < World.Entities.Length; i++) {
			BitSet entity = World.Entities[i];
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

	public IEnumerator<(T1, T2, T3)> Execute() {
		for (int i = 0; i < World.Entities.Length; i++) {
			BitSet entity = World.Entities[i];
			if (!entity.Has(Component1.ID) || !entity.Has(Component2.ID) || !entity.Has(Component3.ID)) continue;
			yield return (Component1.Data.Get(i), Component2.Data.Get(i), Component3.Data.Get(i));
		}
	}
}