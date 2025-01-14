using System.Collections.Generic;

namespace Core.ECS;

public static class Query<T> 
		where T : struct {
	public static List<T> values = [];
	public static int Count = 0;
	static bool Dirty = true;

	public static T[] Execute(World world) {
		var component = world.GetComponent<T>();
		Dirty = component.Dirty;
		if (Dirty) {
			values = [];
			for (int i = 0; i < world.Entities.Length; i++) {
				if (!world.Entities[i].Has(component.Offset)) continue;
				
				values.Add(world.GetComponent<T>().DataStore.Get(i));
			}
			Count = values.Count;
		}

		return values.ToArray();
	}
}