using ECS.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ECS;

public class World {
    public struct EntityMap {
        internal BitArray[] componentFlags;
        internal World world;

        public Entity this[int index] {
            get => new Entity(world, index);
            set => new Entity(world, index).CopyFrom(value);
        } 
    }

    internal static int lastWorldId = -1;

    internal int nextEntityId = 0;
    internal Stack<int> freeEntityIds;
    
    internal List<SparseSetBase> components = [];
    internal List<Type> componentTypes = [];
    internal BitArray dirtyComponents = new BitArray(0);

    public int Id;
    public int ComponentCount;
    public EntityMap Entities;

    public World(int initial_size = 256) {
        Id = ++lastWorldId;
        ComponentCount = 0;
        freeEntityIds = new Stack<int>();

        Entities = new EntityMap() {
            componentFlags = [],
            world = this,
        };

        Array.Resize(ref Entities.componentFlags, initial_size);
        for (int i = 0; i < initial_size; i++) Entities.componentFlags[i] = new BitArray(ComponentCount);
    }

    public void AddComponent<T>() where T : struct {
        ComponentStorage<T>.AddTo(this);
        components.Add(GetComponent<T>().Data);
        componentTypes.Add(typeof(T));
        ComponentCount++;
        dirtyComponents.Length = ComponentCount;
        
        for (int i = 0; i < Entities.componentFlags.Length; i++) {
            Entities.componentFlags[i].Length = ComponentCount;
        }
    }

    public ComponentRecord GetComponent<T>() where T : struct => ComponentStorage<T>.GetFrom(this);

    internal int GetFreeId() {
        int free_id = freeEntityIds.TryPop(out int id) ? id : nextEntityId++;
        
        if (free_id >= Entities.componentFlags.Length) {
            int old_length = Entities.componentFlags.Length;
            
            Array.Resize(ref Entities.componentFlags, Math.Max(free_id, old_length * 2));
            for (int i = old_length; i < Entities.componentFlags.Length; i++) Entities.componentFlags[i] = new BitArray(ComponentCount);
        }

        return free_id;
    }

    public int[] Query<TWith>() where TWith : struct {
        return QueryCache.Execute<TWith>(this);
    }

    public int[] Query<TWith, TWithout>() where TWith : struct where TWithout : struct {
        return QueryCache.Execute<TWith, TWithout>(this);
    }
}

public ref struct Entity {
    internal readonly World world;
    internal ref BitArray componentFlags;
    
    public readonly int Id;

    public Entity(World hostWorld, int? id = null) {
        world = hostWorld;
        Id = id ?? hostWorld.GetFreeId();
        componentFlags = ref hostWorld.Entities.componentFlags[Id];
    }

    public bool Has<T>() where T : struct {
        int index = world.GetComponent<T>().Id;
        return componentFlags[index];
    }

    public T? Get<T>() where T : struct {
        return (T?)world.GetComponent<T>().Data.TryGet(Id);
    }

    public Entity Set<T>(T data) where T : struct {
        ComponentRecord component = world.GetComponent<T>();
        if (!componentFlags[component.Id]) world.dirtyComponents[component.Id] = true;
        componentFlags[component.Id] = true;
        component.Data.Set(Id, data);
        return this;
    }

    public Entity Remove<T>() where T : struct {
        ComponentRecord component = world.GetComponent<T>();
        if (componentFlags[component.Id]) world.dirtyComponents[component.Id] = true;
        componentFlags[component.Id] = false;
        component.Data.Delete(Id);
        return this;
    }

    public void Destroy() {
        if (componentFlags.HasAnySet()) {
            foreach (SparseSetBase component in world.components) component.Delete(Id);
        }

        componentFlags.SetAll(false);
        world.freeEntityIds.Push(Id);
    }

    public void CopyFrom(Entity incoming) {
        for (int i = 0; i < world.ComponentCount; i++) {
            bool shouldHave = incoming.componentFlags[i];

            componentFlags[i] = shouldHave;
            if (shouldHave) {
                world.components[i].Set(Id, incoming.world.components[i].TryGet(incoming.Id));
                return;
            }

            world.components[i].Delete(Id);
        }
    }
}

public static class ComponentStorage<T> where T : struct {
    internal static int[] id = [];
    internal static SparseSet<T>?[] components = [];

    internal static void AddTo(World world) {
        if (world.Id >= id.Length) {
            int old_length = id.Length;
            Array.Resize(ref id, World.lastWorldId + 1);
            Array.Fill(id, -1, id.Length - old_length, old_length);
            Array.Resize(ref components, World.lastWorldId + 1);
        }

        id[world.Id] = world.ComponentCount;
        components[world.Id] = new SparseSet<T>();
    }

    internal static ComponentRecord GetFrom(World world) {
        return new ComponentRecord(id[world.Id], components[world.Id]);
    } 
}

public struct ComponentRecord(int id, SparseSetBase data) {
    public int Id = id;
    public SparseSetBase Data = data;
}
