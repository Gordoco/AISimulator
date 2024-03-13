using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class which ensures no collisions happen in environment by detecting them a frame early and taking necessary prevention measures
 */
public class ReactiveCollisionPrevention
{
    public ReactiveCollisionPrevention(GameObject obj, Collider objCollider, DynamicCoordinateGrid mapping)
    {
        Bounds bounds = objCollider.bounds;
        float radius = Vector2.Distance(new Vector2(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y), new Vector2(bounds.center.x, bounds.center.y));
        RaycastHit[] hits = Physics.SphereCastAll(bounds.center, radius, Vector3.down, 0f);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject != obj)
            {
                //SOLVE COLLISION
                Debug.Log("WE HAVE A COLLISION BOYS: " + hits[i].collider.gameObject.name);
                if (hits[i].collider.gameObject.GetComponent<Agent>()) HandleAgentAgentCollision(obj, hits[i].collider, radius, mapping);
                else HandleAgentWallCollision(obj, hits[i].collider);
            }
        }
    }

    /// <summary>
    /// Given two circles, calculates the intersection points for the three cases: circles don't overlap (no points),
    /// circles intersect at exactly 1 point, circles intersect at exactly 2 points;
    /// </summary>
    /// <param name="radius1"></param>
    /// <param name="center1"></param>
    /// <param name="radius2"></param>
    /// <param name="center2"></param>
    /// <returns></returns>
    Vector2[] CircleCircleIntersection(float radius1, Vector2 center1, float radius2, Vector2 center2)
    {
        float r1Square = radius1 * radius1;
        float r2Square = radius2 * radius2;
        float dist = Vector2.Distance(center1, center2);

        if (dist > radius1 + radius2) return new Vector2[0];

        float d = Mathf.Sqrt(Mathf.Pow(center1.x - center2.x, 2) + Mathf.Pow(center1.y - center2.y, 2));
        float l = Mathf.Pow(d, 2) / (2 * d);
        float h = Mathf.Sqrt(Mathf.Pow(radius1, 2) - Mathf.Pow(l, 2));

        /*float term1 = ((radius1 * radius1) - (radius2 * radius2)) / (2 * (dist * dist));
        float term2_help = (2 * ((r1Square + r2Square) / (dist * dist))) - (Mathf.Pow(r1Square - r2Square, 2) / Mathf.Pow(dist, 4)) - 1;
        float term2 = (1f / 2f) * Mathf.Sqrt(term2_help);*/

        float x1 = ((l / d) * (center2.x - center1.x)) + ((h/d) * (center2.y - center1.y)) + center1.x;
        float y1 = ((l / d) * (center2.y - center1.y)) - ((h / d) * (center2.x - center1.x)) + center1.y;

        float x2 = ((l / d) * (center2.x - center1.x)) - ((h / d) * (center2.y - center1.y)) + center1.x;
        float y2 = ((l / d) * (center2.y - center1.y)) + ((h / d) * (center2.x - center1.x)) + center1.y;

        if (x1 == x2 && y1 == y2)
        {
            Vector2[] arr = new Vector2[1];
            arr[0] = new Vector2(x1, y1);
            return arr;
        }
        else
        {
            Vector2[] arr = new Vector2[2];
            arr[0] = new Vector2(x1, y1);
            arr[1] = new Vector2(x2, y2);
            //Debug.Log("INTERSECTION1: " + arr[0] +  " INTERSECTION2: " + arr[1]);
            //Debug.DrawLine(new Vector3(arr[0].x, -50, arr[0].y), new Vector3(arr[0].x, 50, arr[0].y), Color.magenta, 2);
            //Debug.DrawLine(new Vector3(arr[1].x, -50, arr[1].y), new Vector3(arr[1].x, 50, arr[1].y), Color.magenta, 2);
            return arr;
        }
    }

    /// <summary>
    /// Handles an agent on agent collision by using a "right hand" convention and moving to a point where their bounding circles are tangent
    /// </summary>
    /// <param name="obj">Agent owning this script</param>
    /// <param name="hitCollider">Collider of opposing agent</param>
    private void HandleAgentAgentCollision(GameObject obj, Collider hitCollider, float radius, DynamicCoordinateGrid mapping)
    {
        Bounds selfBounds = obj.GetComponent<Collider>().bounds;
        Bounds otherBounds = hitCollider.bounds;

        Vector2 selfCenter = new Vector2(selfBounds.center.x, selfBounds.center.z);
        Vector2 otherCenter = new Vector2(otherBounds.center.x, otherBounds.center.z);

        Debug.Log("Adjusting Position, Overlapped Locations: " + selfBounds.center);

        Vector2[] intersectionPoints = CircleCircleIntersection(radius, selfCenter, radius, otherCenter);

        if (intersectionPoints.Length == 0) Debug.Log("ERROR: ReactiveCollisionPrevention.cs - Overlap detected but bounding circles not overlapping");
        else if (intersectionPoints.Length == 1) return; //OK "collision" because they are simply tangent, will be handled next frame
        else
        {
            //Right hand convention assuming intersectionPoints[1] is on the "right" relative to owning agent
            Vector2 point = intersectionPoints[1];

            Vector2 dir = point - otherCenter;
            dir.Normalize();
            Vector2 locationToMove2D = otherCenter + (dir * (2 * radius));

            //Vector3 locationToMove = new Vector3(locationToMove2D.x, obj.transform.position.y, locationToMove2D.y);

            //TODO: Add an additional SphereCastAll at this point and if colliding flag agent as "CANTMOVE"
            mapping.Move(locationToMove2D, obj.GetComponent<Agent>(), false, null, false);

            //DEBUG: Print
            //Print(obj, radius, selfCenter, radius, otherCenter, locationToMove2D);
        }
    }

    private void Print(GameObject obj, float origRadius, Vector2 origCenter, float otherRadius, Vector2 otherCenter, Vector2 newCenter)
    {
        int segments = 60;
        float time = 2;
        float y = 2.5f;
        DrawCircle(new Vector3(origCenter.x, y, origCenter.y), origRadius, segments, Color.black, time);
        DrawCircle(new Vector3(otherCenter.x, y, otherCenter.y), otherRadius, segments, Color.red, time);
        DrawCircle(new Vector3(newCenter.x, y, newCenter.y), origRadius, segments, Color.blue, time);
        //Debug.Break();
    }

    public static void DrawCircle(Vector3 position, float radius, int segments, Color color, float time)
    {
        // If either radius or number of segments are less or equal to 0, skip drawing
        if (radius <= 0.0f || segments <= 0)
        {
            return;
        }

        // Single segment of the circle covers (360 / number of segments) degrees
        float angleStep = (360.0f / segments);

        // Result is multiplied by Mathf.Deg2Rad constant which transforms degrees to radians
        // which are required by Unity's Mathf class trigonometry methods

        angleStep *= Mathf.Deg2Rad;

        // lineStart and lineEnd variables are declared outside of the following for loop
        Vector3 lineStart = Vector3.zero;
        Vector3 lineEnd = Vector3.zero;

        for (int i = 0; i < segments; i++)
        {
            // Line start is defined as starting angle of the current segment (i)
            lineStart.x = Mathf.Cos(angleStep * i);
            lineStart.z = Mathf.Sin(angleStep * i);

            // Line end is defined by the angle of the next segment (i+1)
            lineEnd.x = Mathf.Cos(angleStep * (i + 1));
            lineEnd.z = Mathf.Sin(angleStep * (i + 1));

            // Results are multiplied so they match the desired radius
            lineStart *= radius;
            lineEnd *= radius;

            // Results are offset by the desired position/origin 
            lineStart += position;
            lineEnd += position;

            lineStart.y = 2.5f;
            lineEnd.y = 2.5f;

            // Points are connected using DrawLine method and using the passed color
            Debug.DrawLine(lineStart, lineEnd, color, time);
        }
    }

    private void HandleAgentWallCollision(GameObject obj, Collider hitCollider)
    {

    }
}
