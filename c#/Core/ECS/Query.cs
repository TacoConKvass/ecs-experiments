using System.Collections.Generic;

namespace Core.ECS;

public static class Query<T> 
		where T : struct {

	public static IEnumerable<T> Execute(World world) {
		var component = world.GetComponent<T>();
		for (int i = 0; i < world.Entities.Length; i++) {
			if (!world.Entities[i].Has(component.Offset)) continue;
			yield return component.DataStore.Data[i];
		}
	}
}