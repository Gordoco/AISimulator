using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* A data structure used to keep a locally valid coordinate grid for querying
*/
public class DynamicCoordinateGrid : MonoBehaviour
{

    public int width;
    public int height;

    public Vector3 Origin = new Vector3(0, 0, 0);
    public int[] gridCorner = new int[2];

    private Vector2 currentLocation = new Vector2(0, 0);
    private Vector2 localOrigin = new Vector2(0, 0);
    private List<List<int>> grid;
    private Vector2 lastMappedPos = new Vector2();

    public bool bQuadTreeNeedsRegen = true;

    /**
     * #### void Move(Vector2, int[][])
     * Updates coordinate grid based on a directional move and the result of an environmental scan
     */
    //public void Move(Vector2 dir, int[][] localMap, float inverseMoveSpeed)
    //{
    public void Move(Vector2 newLoc, Agent owner, bool bTeleport = false, PathPlanner planner = null, bool bCheckCollision = true, bool bPrint = false)
    {
        if (bCheckCollision)
        {
            ReactiveCollisionPrevention CP = new ReactiveCollisionPrevention();
            Vector3 CollisionDir;
            if (!CP.CheckIfShouldMove(owner.gameObject, toVector3(owner.gameObject.transform.position.y, newLoc), this, out CollisionDir))
            {
                if (planner != null) planner.CancelPath(true);
                return;
            }
        }
        //Debug.Log("BEFORE MOVE: " + oldLoc);

        if (bTeleport && planner == null)
        {
            Debug.Log("DCG_Move: ERROR Cannot teleport without specifying a planner");
            return;
        }

        if (bTeleport)
        {
            planner.CancelPath();
        }

        int[] intVect = { -1 };

        int dirX = (int)Mathf.Round(newLoc.x - lastMappedPos.x);
        for (int k = 0; k < Mathf.Abs(dirX); k++)
        {
            //Dynamic Reallocation
            if (dirX > 0) //RIGHT
            {
                if (3 + GetConversionFactor().x == width)
                {
                    //Add row to the top
                    grid.Insert(0, new List<int>());
                    for (int i = 0; i < width; i++)
                    {
                        grid[0].Add((int)MappingIDs.Undefined);
                    }
                    localOrigin.y += 1;

                    //Add Column to the right
                    for (int i = 0; i < grid.Count; i++)
                    {
                        grid[i].Add((int)MappingIDs.Undefined);
                    }
                }
                //Debug.Log("RIGHT");
                currentLocation.x += 1;
                int[] temp = { (int)lastMappedPos.x + 1, (int)lastMappedPos.y };
                intVect = temp;
            }
            else if (dirX < 0) //LEFT
            {
                if (GetConversionFactor().x == 0)
                {
                    //Add row to the bottom
                    grid.Add(new List<int>());
                    for (int i = 0; i < width; i++)
                    {
                        grid[grid.Count - 1].Add((int)MappingIDs.Undefined);
                    }
                    gridCorner[1] -= 1;

                    //Add Column to the left
                    for (int i = 0; i < grid.Count; i++)
                    {
                        grid[i].Insert(0, (int)MappingIDs.Undefined);
                    }
                    localOrigin.x += 1;
                    gridCorner[0] -= 1;
                }
                //Debug.Log("LEFT");
                currentLocation.x -= 1;
                int[] temp = { (int)lastMappedPos.x - 1, (int)lastMappedPos.y };
                intVect = temp;
            }
            width = grid[0].Count;
            height = grid.Count;
            if (!bTeleport)
            {
                SetLocalValues(owner.ScanArea(intVect));
                lastMappedPos = new Vector2(intVect[0], intVect[1]);
            }
        }

        int dirY = (int)Mathf.Round(newLoc.y - lastMappedPos.y);
        for (int k = 0; k < Mathf.Abs(dirY); k++)
        {
            if (dirY > 0) //Up
            {
                if (GetConversionFactor().y == 0)
                {
                    //Add row to the top
                    grid.Insert(0, new List<int>());
                    for (int i = 0; i < width; i++)
                    {
                        grid[0].Add((int)MappingIDs.Undefined);
                    }
                    localOrigin.y += 1;

                    //Add Column to the right
                    for (int i = 0; i < grid.Count; i++)
                    {
                        grid[i].Add((int)MappingIDs.Undefined);
                    }
                }
                //Debug.Log("UP");
                currentLocation.y -= 1;
                int[] temp = { (int)lastMappedPos.x, (int)lastMappedPos.y + 1 };
                intVect = temp;
            }
            else if (dirY < 0) //Down
            {
                if (3 + GetConversionFactor().y == height)
                {
                    //Add row to the bottom
                    grid.Add(new List<int>());
                    for (int i = 0; i < width; i++)
                    {
                        grid[grid.Count - 1].Add((int)MappingIDs.Undefined);
                    }
                    gridCorner[1] -= 1;

                    //Add Column to the left
                    for (int i = 0; i < grid.Count; i++)
                    {
                        grid[i].Insert(0, (int)MappingIDs.Undefined);
                    }
                    localOrigin.x += 1;
                    gridCorner[0] -= 1;
                }
                //Debug.Log("DOWN");
                currentLocation.y += 1;
                int[] temp = { (int)lastMappedPos.x, (int)lastMappedPos.y - 1 };
                intVect = temp;
            }
            width = grid[0].Count;
            height = grid.Count;
            if (intVect.Length != 2) Debug.Log("DCG_Move: ERROR WITH INTEGER VECTOR CALCULATION");
            if (!bTeleport)
            {
                SetLocalValues(owner.ScanArea(intVect));
                lastMappedPos = new Vector2(intVect[0], intVect[1]);
            }
            //if (bPrint) Print();
        }

        //Physically Move
        owner.gameObject.transform.position = toVector3(owner.gameObject.transform.position.y, newLoc);
        //Debug.Log("AFTER MOVE: " + owner.gameObject.transform.position);

        int[] temp2 = { (int)Mathf.Round(toVector2(owner.gameObject.transform.position).x), (int)Mathf.Round(toVector2(owner.gameObject.transform.position).y) };
        intVect = temp2;
        SetLocalValues(owner.ScanArea(intVect));
        lastMappedPos = new Vector2(intVect[0], intVect[1]);
        //DEBUG Unit Tests
        //Print(0.05f);
    }

    bool bDraw = false;
    Vector3 center;
    float radius;
    public void SetupGizmoDraw(Vector3 center, float radius)
    {
        bDraw = true;
        this.center = center;
        this.radius = radius;
    }

    private void OnDrawGizmos()
    {
        if (bDraw)
        {
            Gizmos.DrawSphere(center, radius);
            bDraw = false;
        }
    }

    /**
     * #### void SetLocalValues()
     * Iterativly sets values through the grid at newly mapped locations
     */
    private void SetLocalValues(int[][] localMap)
    {
        //Apply New Local Map
        for (int i = 0; i < localMap.Length; i++)
        {
            for (int j = 0; j < localMap[0].Length; j++)
            {
                if ((MappingIDs)grid[i + (int)GetConversionFactor().y][j + (int)GetConversionFactor().x] != (MappingIDs)localMap[i][j])
                {
                    bQuadTreeNeedsRegen = true;
                }
                grid[i + (int)GetConversionFactor().y][j + (int)GetConversionFactor().x] = localMap[i][j];
            }
        }
    }

    public void Init(Agent owner)
    {
        gridCorner[0] = -1;
        gridCorner[1] = -1;
        //Initialize start of grid
        grid = new List<List<int>>();
        for (int i = 0; i < 3; i++)
        {
            grid.Add(new List<int>());
            for (int j = 0; j < 3; j++)
            {
                grid[i].Add((int)MappingIDs.Undefined);
            }
        }
        localOrigin = new Vector2(0, 0);
        width = 3;
        height = 3;

        width = grid[0].Count;
        height = grid.Count;
        int[] intVect = { (int)owner.gameObject.transform.position.x, (int)owner.gameObject.transform.position.z };
        SetLocalValues(owner.ScanArea(intVect));
        lastMappedPos = new Vector2(intVect[0], intVect[1]);
    }

    /**
     * #### Vector2 GetConversionFactor()
     * Returns the x and y conversion values to go from array index to object space
     */
    public Vector2 GetConversionFactor()
    {
        if ((int)currentLocation.x + (int)localOrigin.x < 0 || (int)currentLocation.y + (int)localOrigin.y < 0) Debug.Log("ERRORRORORORO");
        return new Vector2((int)currentLocation.x + (int)localOrigin.x, (int)currentLocation.y + (int)localOrigin.y);
    }

    public Vector3 toVector3(float y, Vector2 input)
    {
        return new Vector3(input.x, y, input.y);
    }

    public Vector2 toVector2(Vector3 input)
    {
        return new Vector2(input.x, input.z);
    }

    public MappingIDs GetMapping(int x, int y)
    {
        return (MappingIDs)grid[grid.Count - 1 - y][x];
    }

    /**
     * #### void Print()
     * Debugging method which prints a grid representation to console
     */
    public void Print(float inverseMoveSpeed = 0.2f)
    {
        string toPrint = "";
        for (int i = 0; i < grid.Count; i++)
        {
            for (int j = 0; j < grid[0].Count; j++)
            {
                toPrint += grid[i][j] + " ";
                Color lineCol;
                if ((MappingIDs)grid[i][j] == MappingIDs.Free) lineCol = Color.green;
                else lineCol = Color.red;
                Vector3 init = new Vector3(j + Origin.x + gridCorner[0], 5, (grid.Count - 1) - i + Origin.z + gridCorner[1]);
                Vector3 end = new Vector3(j + Origin.x + gridCorner[0], -5, (grid.Count - 1) - i + Origin.z + gridCorner[1]);
                Debug.DrawLine(init, end, lineCol, inverseMoveSpeed);
            }
            toPrint += "\n";
        }
        /*Debug.Log(toPrint);
        Debug.Log("W: " + width + " || H: " + height);
        Debug.Log("X: " + currentLocation.x + " || Y: " + currentLocation.y);
        Debug.Log("ConvX: " + GetConversionFactor().x + " || ConvY: " + GetConversionFactor().y);
        QuadTree QT = new QuadTree();
        QT.Construct(this, Origin, inverseMoveSpeed);
        QuadTreeNode myNode = QT.GetNode(new Vector2(transform.position.x, transform.position.z));
        List<QuadTreeNode> neighbors = null;
        if (myNode != null) neighbors = myNode.GetDirections();
        if (neighbors != null)
            for (int i = 0; i < neighbors.Count; i++)
            {
                if (neighbors[i] != null)
                {
                    neighbors[i].colorOverride = Color.magenta;
                    neighbors[i].Print(inverseMoveSpeed);
                }
            }
        if (myNode != null)
        {
            myNode.colorOverride = Color.black;
            myNode.Print(inverseMoveSpeed);
        }*/
    }
}
