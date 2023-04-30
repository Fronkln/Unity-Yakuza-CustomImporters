using UnityEngine.Rendering;

public struct VecStorage {
    VertexAttributeFormat Component;
    int NumComponents;

    public VecStorage(VertexAttributeFormat component, int numComponents) {
        if (numComponents < 2 || numComponents > 4) {
            throw new System.ArgumentOutOfRangeException("numComponents " + numComponents + " not between 2 and 4");
        }

        Component = component;
        NumComponents = numComponents;
    }
}
