using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class which ensures no collisions happen in environment by detecting them a frame early and taking necessary prevention measures
 */
public class ReactiveCollisionPrevention
{

    private Vector3 oldLoc;

    public ReactiveCollisionPrevention() { }

    public ReactiveCollisionPrevention(GameObject obj, Vector3 oldLoc, Collider objCollider, DynamicCoordinateGrid mapping, bool bPrint = false)
    {
        this.oldLoc = oldLoc;
        Bounds bounds = objCollider.bounds;
        float radius = Vector3.Distance(bounds.center + bounds.extents, bounds.center);
        Vector3 point = bounds.center;
        //point.y += 20;
        RaycastHit[] hits = Physics.SphereCastAll(point, radius, Vector3.down, 0f);
        //mapping.SetupGizmoDraw(bounds.center, radius);
        //Debug.Log("NUM HITS: " + hits.Length);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject != obj)
            {
                //SOLVE COLLISION
                //Debug.Log("WE HAVE A COLLISION BOYS: " + hits[i].collider.gameObject.name);
                if (hits[i].collider.gameObject.GetComponent<Agent>()) { }// HandleAgentAgentCollision(obj, hits[i].collider, radius, mapping, bPrint);
                else
                {
                    //Debug.Log("WALL");
                    Vector3[] points = new Vector3[2];

                    Vector3 normal = GetCorrectNormalForSphere(hits[i], obj.transform.forward);
                    RaycastHit hit;
                    Vector3 point1 = bounds.center;
                    if (Physics.Raycast(point1, -normal, out hit, radius, 1 << 8))
                    {
                        Debug.Log("ADJUSTING");
                        float x = Vector3.Distance(point1, hit.point);
                        float y = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(x, 2));

                        Vector3 dir = Vector3.Cross(normal, Vector3.up).normalized;
                        points[0] = hit.point + (dir * y);
                        points[1] = hit.point - (dir * y);

                        HandleAgentWallCollision(obj, hits[i].collider, normal, points, radius, mapping, bPrint);

                        if (bPrint) DrawCircle(bounds.center, radius, 60, Color.blue, 5);
                    }
                    else
                    {
                    }
                }
            }
        }
    }

    public bool CheckIfShouldMove(GameObject owner, Vector3 newPos, DynamicCoordinateGrid mapping, out Vector3 dir)
    {
        Bounds bounds = owner.GetComponent<Collider>().bounds;
        float radius = Vector3.Distance(bounds.center + bounds.extents, bounds.center);
        RaycastHit[] hits = Physics.SphereCastAll(new Vector3(newPos.x, newPos.y + 20, newPos.z), radius, Vector3.down, 20f);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject == GameObject.FindGameObjectWithTag("Goal")) Debug.Log("WHOOPS GOAL IS COLLIDING");
            if (hits[i].collider.gameObject != owner)
            {
                //DrawCircle(newPos, radius, 60, Color.red, 1);
                if (hits[i].point != Vector3.zero) dir = (hits[i].point - owner.transform.position).normalized;
                else dir = Vector3.zero;
                return false;
            }
        }
        dir = Vector3.zero;
        if (hits.Length > 1) return false;
        //(newPos, radius, 60, Color.green, 1);
        return true;
    }

    private void UndoMove(DynamicCoordinateGrid mapping, GameObject obj)
    {
        Debug.Log(oldLoc);
        //mapping.Move(mapping.toVector2(oldLoc), obj.GetComponent<Agent>(), true, obj.GetComponent<Agent>().GetPlanner(), false);
    }

    private bool CheckForSecondaryCollision(Vector3 newLoc, float radius, Bounds objBounds, GameObject obj)
    {
        Bounds bounds = obj.GetComponent<Collider>().bounds;
        newLoc.y += 20f;
        RaycastHit[] hits = Physics.SphereCastAll(newLoc, radius, Vector3.down, 20f);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject != obj)
            {
                Debug.Log("CANT MOVE: HIT " + hits[i].collider.gameObject.name);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Calculates the surface normal for CapsuleCast and SphereCast
    /// </summary>
    /// <param name="hit">original hit</param>
    /// <param name="dir">original direction of the raycast</param>
    /// <returns>correct normal</returns>
    /// <remarks>https://forum.unity.com/threads/spherecast-capsulecast-raycasthit-normal-is-not-the-surface-normal-as-the-documentation-states.275369/</remarks>
    public Vector3 GetCorrectNormalForSphere(RaycastHit hit, Vector3 dir)
    {
        if (hit.collider is MeshCollider)
        {
            var collider = hit.collider as MeshCollider;
            var mesh = collider.sharedMesh;
            var tris = mesh.triangles;
            var verts = mesh.vertices;

            var v0 = verts[tris[hit.triangleIndex * 3]];
            var v1 = verts[tris[hit.triangleIndex * 3 + 1]];
            var v2 = verts[tris[hit.triangleIndex * 3 + 2]];

            var n = Vector3.Cross(v1 - v0, v2 - v1).normalized;

            
            return hit.transform.TransformDirection(n);
        }
        else
        {
            RaycastHit result;
            hit.collider.Raycast(new Ray(hit.point - dir * 0.01f, dir), out result, 0.011f);
            return result.normal;
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
    private void HandleAgentAgentCollision(GameObject obj, Collider hitCollider, float radius, DynamicCoordinateGrid mapping, bool bPrint = false)
    {
        Bounds selfBounds = obj.GetComponent<Collider>().bounds;
        Bounds otherBounds = hitCollider.bounds;

        Vector2 selfCenter = new Vector2(selfBounds.center.x, selfBounds.center.z);
        Vector2 otherCenter = new Vector2(otherBounds.center.x, otherBounds.center.z);

        //Debug.Log("Adjusting Position, Overlapped Locations: " + selfBounds.center);

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
            // (!CheckForSecondaryCollision(locationToMove2D, radius, selfBounds, obj)) mapping.Move(locationToMove2D, obj.GetComponent<Agent>(), false, null, bPrint);

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

    public static Vector3[] GetCircleIntersections(Vector3 position, float radius, int segments)
    {
        // If either radius or number of segments are less or equal to 0, skip drawing
        if (radius <= 0.0f || segments <= 0)
        {
            return null;
        }

        // Single segment of the circle covers (360 / number of segments) degrees
        float angleStep = (360.0f / segments);
        RaycastHit[] hits = new RaycastHit[2]; //Expect at most 2 hits
        int count = 0; //Index for the next hit
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
            Vector3 dir = lineEnd - lineStart;
            if (Physics.Raycast(lineStart, dir.normalized, out hits[count], dir.magnitude)) count++;
        }
        Vector3[] locs = new Vector3[2];
        if (count == 0) return null;
        for (int i = 0; i < count; i++) locs[i] = hits[i].point; //Setup vectors based on how many successful casts
        return locs;
    }

    private void HandleAgentWallCollision(GameObject obj, Collider hitCollider, Vector3 normal, Vector3[] circleIntercepts, float radius, DynamicCoordinateGrid mapping, bool bPrint = false)
    {
        Vector3 moveDir = obj.transform.forward;
        Vector3 compPos = obj.transform.position + moveDir;
        Vector3 posToUse = circleIntercepts[0];

        float dist1 = Vector3.Distance(compPos, circleIntercepts[0]);
        float dist2 = Vector3.Distance(compPos, circleIntercepts[1]);

        if (dist1 <= dist2) posToUse = circleIntercepts[0];
        else posToUse = circleIntercepts[1];

        Vector3 destination = posToUse + (normal * (radius + 0.1f));
        Vector3 altDestination = circleIntercepts[1] + (normal * (radius + 0.1f));
        /*Debug.Log("DESTINATION: " + destination);
        Debug.Log("posToUse: " + posToUse);
        Debug.Log("(normal * radius): " + (normal * radius));*/

        if (bPrint)
        {
            Debug.DrawLine(new Vector3(posToUse.x, -5, posToUse.z), new Vector3(posToUse.x, 5, posToUse.z), Color.gray, 5);
            Debug.DrawLine(new Vector3(circleIntercepts[1].x, -5, circleIntercepts[1].z), new Vector3(circleIntercepts[1].x, 5, circleIntercepts[1].z), Color.gray, 5);
        }

        /*if (!CheckForSecondaryCollision(destination, radius, obj.GetComponent<Collider>().bounds, obj)) mapping.Move(new Vector2(destination.x, destination.z), obj.GetComponent<Agent>(), true, obj.GetComponent<Agent>().GetPlanner(), false);
        else if (!CheckForSecondaryCollision(altDestination, radius, obj.GetComponent<Collider>().bounds, obj))
        {
            mapping.Move(new Vector2(altDestination.x, altDestination.z), obj.GetComponent<Agent>(), true, obj.GetComponent<Agent>().GetPlanner(), false);
            //Debug.Break();
        }
        else
        {
            UndoMove(mapping, obj);
        }*/
        //Debug.Break();
    }
}
