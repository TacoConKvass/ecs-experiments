using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ECS;

internal static class QueryCache {
    internal static Dictionary<Type, BitArray>[] maskCache = [];

    internal static MethodInfo method = typeof(QueryCache).GetMethod("MakeMask", BindingFlags.NonPublic | BindingFlags.Static)!;
    
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

    internal static Dictionary<Type, int[]>[] withQueryCache = [];

    internal static int[] Execute<TWith>(World world) where TWith : struct {
        int id = world.Id;
        if (id >= withQueryCache.Length) {
            Array.Resize(ref withQueryCache, id + 1);
            withQueryCache[id] = [];
        }
        
        BitArray mask = MakeMask<TWith>(world);
        BitArray dirty = new BitArray(mask).And(world.dirtyComponents);

        if (!dirty.HasAnySet() && withQueryCache[id].TryGetValue(typeof(TWith), out int[] entities)) return entities;

        List<int> entities_to_return = [];
        for (int i = 0; i < world.Entities.componentFlags.Length; i++) {
            BitArray entity_mask = world.Entities.componentFlags[i];
            BitArray included_mask = new BitArray(entity_mask).And(mask).Not().Xor(mask);
            
            if (included_mask.HasAllSet()) 
                entities_to_return.Add(i);
        }

        entities = entities_to_return.ToArray();
        withQueryCache[id][typeof(TWith)] = entities;
        return entities;
    }
    
    internal static Dictionary<Type, int[]>[] with_withoutQueryCache = [];

    internal static int[] Execute<TWith, TWithout>(World world) where TWith : struct where TWithout : struct {
        int id = world.Id;
        if (id >= with_withoutQueryCache.Length) {
            Array.Resize(ref with_withoutQueryCache, id + 1);
            with_withoutQueryCache[id] = [];
        }
        
        BitArray mask = MakeMask<TWith>(world);
        BitArray dirty = new BitArray(mask).And(world.dirtyComponents);

        if (!dirty.HasAnySet() && with_withoutQueryCache[id].TryGetValue(typeof(TWith), out int[] entities)) return entities;

        List<int> entities_to_return = [];
        for (int i = 0; i < world.Entities.componentFlags.Length; i++) {
            BitArray entity_mask = world.Entities.componentFlags[i];
            BitArray included_mask = new BitArray(entity_mask).And(mask).Not().Xor(mask);
            BitArray excluded_mask = new BitArray(entity_mask).And(mask);

            if (included_mask.HasAllSet() && !excluded_mask.HasAnySet()) 
                entities_to_return.Add(i);
        }

        entities = entities_to_return.ToArray();
        with_withoutQueryCache[id][typeof(TWith)] = entities;
        return entities;
    }
}

public struct And<T1, T2> { }
