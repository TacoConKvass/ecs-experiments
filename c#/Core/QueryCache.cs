using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ECS;

internal static class QueryCache {
	internal static Dictionary<Type, BitArray>[] maskCache = [];

	internal static BitArray MakeMask<T>(World world) where T : struct {
		int id = world.Id;
		if (id >= maskCache.Length) {
			Array.Resize(ref maskCache, id + 1);
			maskCache[id] = [];
		}

		Type type = typeof(T);

		if (maskCache[id].TryGetValue(type, out BitArray mask)) return mask;
		
		mask = new BitArray(world.ComponentCount);
		
		if (type.IsGenericType) {
			if (type.GetGenericTypeDefinition() == typeof(And<,>).GetGenericTypeDefinition()) {
				Type[] types = type.GetGenericArguments();
				MethodInfo method = typeof(QueryCache).GetMethod("MakeMask", BindingFlags.NonPublic | BindingFlags.Static)!;

				mask.Or((BitArray)method.MakeGenericMethod(types[0]).Invoke(null, [world])!);
				mask.Or((BitArray)method.MakeGenericMethod(types[1]).Invoke(null, [world])!);
				
				maskCache[id][type] = mask;
				return mask;
			}
		}

		mask[world.GetComponent<T>().Id] = true;
		maskCache[id][type] = mask;
		return mask;
	}

	internal static Dictionary<Type, int[]>[] queryCache = [];

	internal static int[] Execute<TWith>(World world) where TWith : struct {
		int id = world.Id;
		if (id >= queryCache.Length) {
			Array.Resize(ref queryCache, id + 1);
			queryCache[id] = [];
		}
		
		BitArray mask = MakeMask<TWith>(world);
		BitArray dirty = new BitArray(mask).And(world.dirtyComponents);

		if (!dirty.HasAnySet() && queryCache[id].TryGetValue(typeof(TWith), out int[] entities)) return entities;

		List<int> entities_to_return = [];
		for (int i = 0; i < world.Entities.componentFlags.Length; i++) {
			BitArray entity_mask = world.Entities.componentFlags[i];
			BitArray included_mask = new BitArray(entity_mask).Not().Xor(mask);
			
			if (included_mask.HasAllSet()) 
				entities_to_return.Add(i);
		}

		entities = entities_to_return.ToArray();
		queryCache[id][typeof(TWith)] = entities;
		return entities;
	}
}

public struct And<T1, T2> { }