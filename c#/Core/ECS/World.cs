using System.Collections;
using System.Runtime.InteropServices.Marshalling;

namespace Core.ECS;

public class World() {
    private byte[] entityFlags = [];
    private int componentCount = 0;
    public void RegisterComponent<T>() where T : struct { 
        Component<T>.ID = componentCount++;
    }

    public void GetComponent<T>() where T : struct { }
}

public static class Component<T> where T : struct {
    public static int ID;
}

struct Entity(int id) {
    public int ID = id;
    public Entity AddComponent() { return this; }
}