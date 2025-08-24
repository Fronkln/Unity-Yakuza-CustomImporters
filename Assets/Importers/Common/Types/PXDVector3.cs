using UnityEngine;

public struct PXDVector3
{
    public float x; 
    public float y; 
    public float z;

    public PXDVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static implicit operator Vector3(PXDVector3 pxdvec)
    {
        return new Vector3(-pxdvec.x, pxdvec.y, pxdvec.z);
    }
}
