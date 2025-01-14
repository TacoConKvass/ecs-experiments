using System.Collections.Generic;

namespace Core.ECS;

public static class Query<T> 
		where T : struct {
	public static List<int> values = [];
	public static int Count = 0;
	static bool Dirty = true;

	public static int[] Execute(World world) {
		var component = world.GetComponent<T>();
		Dirty = component.Dirty;
		
		if (!Dirty) return values.ToArray();

		values = [];
		for (int i = 0; i < world.Entities.Length; i++) {
			if (!world.Entities[i].Has(component.Offset)) continue;		
			values.Add(i);
		}
		
		Count = values.Count;

		return values.ToArray();
	}
}

public static class Query<T1, T2>
		where T1 : struct
		where T2 : struct {
	public static List<int> values = [];
	public static int Count = 0;
	static bool Dirty = true;

	public static int[] Execute(World world) {
		var component1 = world.GetComponent<T1>();
		var component2 = world.GetComponent<T2>();
		Dirty = component1.Dirty || component2.Dirty;

		if (!Dirty) return values.ToArray();

		values = [];
		for (int i = 0; i < world.Entities.Length; i++) {
			if (!world.Entities[i].Has(component1.Offset) || !world.Entities[i].Has(component2.Offset)) continue;
			values.Add(i);
		}

		Count = values.Count;

		return values.ToArray();
	}
}


public static class Query<T1, T2, T3>
		where T1 : struct
		where T2 : struct
		where T3 : struct {
	public static List<int> values = [];
	public static int Count = 0;
	static bool Dirty = true;

	public static int[] Execute(World world) {
		var component1 = world.GetComponent<T1>();
		var component2 = world.GetComponent<T2>();
		var component3 = world.GetComponent<T3>();
		Dirty = component1.Dirty || component2.Dirty;

		if (!Dirty) return values.ToArray();

		values = [];
		for (int i = 0; i < world.Entities.Length; i++) {
			if (!world.Entities[i].Has(component1.Offset) || !world.Entities[i].Has(component2.Offset) || !!world.Entities[i].Has(component3.Offset)) continue;
			values.Add(i);
		}

		Count = values.Count;

		return values.ToArray();
	}
}