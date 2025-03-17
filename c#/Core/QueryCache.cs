using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ECS;

internal static class QueryCache {
	internal static Dictionary<Type, BitArray>[] cache = [];

	internal static BitArray MakeMask<T>(World world) where T : struct {
		int id = world.Id;
		if (id >= cache.Length) {
			Array.Resize(ref cache, id + 1);
			cache[id] = [];
		}

		Type type = typeof(T);

		if (cache[id].TryGetValue(type, out BitArray mask)) return mask;
		
		mask = new BitArray(world.ComponentCount);
		
		if (type.IsGenericType) {
			if (type.GetGenericTypeDefinition() == typeof(And<,>).GetGenericTypeDefinition()) {
				Type[] types = type.GetGenericArguments();
				MethodInfo method = typeof(QueryCache).GetMethod("MakeMask", BindingFlags.NonPublic | BindingFlags.Static)!;

				mask.Or((BitArray)method.MakeGenericMethod(types[0]).Invoke(null, [world])!);
				mask.Or((BitArray)method.MakeGenericMethod(types[1]).Invoke(null, [world])!);
				
				cache[id][type] = mask;
				return mask;
			}
		}

		mask[world.GetComponent<T>().Id] = true;
		cache[id][type] = mask;
		return mask;
	}
}

public struct And<T1, T2> { }