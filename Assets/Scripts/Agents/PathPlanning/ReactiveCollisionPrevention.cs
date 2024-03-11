using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class which ensures no collisions happen in environment by detecting them a frame early and taking necessary prevention measures
 */
public class ReactiveCollisionPrevention
{
    public ReactiveCollisionPrevention(GameObject obj, Collider objCollider)
    {
        Bounds bounds = objCollider.bounds;
        RaycastHit[] hits = Physics.BoxCastAll(bounds.center + (Vector3.up * 5), bounds.extents, Vector3.down, obj.transform.rotation, 5f);
        //Debug.Log("START LOCATION: " + (bounds.center + (Vector3.up * 5)) + " FINISH LOCATION: " + (bounds.center + (Vector3.up * 5) + (Vector3.down * 5f)));
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject != obj)
            {
                //SOLVE COLLISION
                Debug.Log("WE HAVE A COLLISION BOYS: " + hits[i].collider.gameObject.name);
                if (hits[i].collider.gameObject.GetComponent<Agent>()) HandleAgentAgentCollision(obj, hits[i].collider);
                else HandleAgentWallCollision(obj, hits[i].collider);
            }
        }
    }

    private bool PointInRectangle(Vector2 point, Bounds rectBounds)
    {
        Vector2 center2D = new Vector2(rectBounds.center.x, rectBounds.center.z);
        Vector2 extents2D = new Vector2(rectBounds.extents.x, rectBounds.extents.z);
        const int lineMult = 999999999;

        Vector2 lineEnd = point + (Vector2.right * lineMult);
        Vector2 corner1 = new Vector2(center2D.x - extents2D.x, center2D.y + extents2D.y);
        Vector2 corner2 = new Vector2(center2D.x + extents2D.x, center2D.y + extents2D.y);
        Vector2 corner3 = new Vector2(center2D.x + extents2D.x, center2D.y - extents2D.y);
        Vector2 corner4 = new Vector2(center2D.x - extents2D.x, center2D.y - extents2D.y);
        int count = 0;
        Vector2 intOut;
        if (Intersects(point, lineEnd, corner1, corner2, out intOut)) count++;
        if (Intersects(point, lineEnd, corner2, corner3, out intOut)) count++;
        if (Intersects(point, lineEnd, corner3, corner4, out intOut)) count++;
        if (Intersects(point, lineEnd, corner4, corner1, out intOut)) count++;
        if (count == 1) return true;
        return false;
    }

    /**
     * bool Intersects(Vector2, Vector2, Vector2, Vector2, out Vector2)
     * Line intersection helper method, takes in two lines and outputs the boolean intersaction and intersection location
     */
    private bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        Vector2 b = a2 - a1;
        Vector2 d = b2 - b1;
        float bDotDPerp = b.x * d.y - b.y * d.x;

        // if b dot d == 0, it means the lines are parallel so have infinite intersection points
        if (bDotDPerp == 0)
            return false;

        Vector2 c = b1 - a1;
        float t = (c.x * d.y - c.y * d.x) / bDotDPerp;
        if (t < 0 || t > 1)
            return false;

        float u = (c.x * b.y - c.y * b.x) / bDotDPerp;
        if (u < 0 || u > 1)
            return false;

        intersection = a1 + t * b;

        return true;
    }

    private void HandleAgentAgentCollision(GameObject obj, Collider hitCollider)
    {

    }

    private void HandleAgentWallCollision(GameObject obj, Collider hitCollider)
    {

    }
}
