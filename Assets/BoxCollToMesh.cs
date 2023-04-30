using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxCollToMesh : MonoBehaviour
{
    public BoxCollider Coll;

    public bool Left;
    public bool Right;
    public bool Down;
    public bool Up;
    public bool Forward;
    public bool Backward;

    public void OnDrawGizmos()
    {
        if (Coll == null)
            return;

        BoxCollider boxCollider = GetComponent<BoxCollider>();

        Bounds localBounds = new Bounds(boxCollider.center, boxCollider.size);

        const float radius = 0.1f;

        Gizmos.color = Color.green;

        //Down
        if (Down)
        {
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.min.y, localBounds.max.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.min.y, localBounds.max.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.min.y, localBounds.min.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.min.y, localBounds.min.z)), radius);
        }

        if (Up)
        {
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.max.y, localBounds.max.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.max.y, localBounds.max.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.max.y, localBounds.min.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.max.y, localBounds.min.z)), radius);
        }

        if (Backward)
        {
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.max.y, localBounds.min.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.min.y, localBounds.min.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.min.y, localBounds.min.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.max.y, localBounds.min.z)), radius);
        }

        //Forward
        if (Forward)
        {
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.max.y, localBounds.max.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.min.y, localBounds.max.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.min.y, localBounds.max.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.max.y, localBounds.max.z)), radius);
        }
        //Left
        if (Left)
        {
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.max.y, localBounds.max.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.min.y, localBounds.max.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.max.y, localBounds.min.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.max.x, localBounds.min.y, localBounds.min.z)), radius);
        }

        if (Right)
        {
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.max.y, localBounds.max.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.min.y, localBounds.max.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.max.y, localBounds.min.z)), radius);
            Gizmos.DrawSphere(transform.TransformPoint(new Vector3(localBounds.min.x, localBounds.min.y, localBounds.min.z)), radius);
        }
    }
}
