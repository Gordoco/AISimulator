using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* #### MappingIDs
* -----
* A collection of labels for mapping to be used in grid location identification
* Undefined: Location has yet to be mapped
* Free: Location is traversable
* Full: Location is un-traversable
* Grounding: Location is traversable and contains a grounding which may be demonstrated
*/
public enum MappingIDs { Undefined, Free, Full, Grounding }

/**
* #### DynamicCoordinateGrid
* -----
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

    /**
     * #### void Move(Vector2, int[][])
     * Updates coordinate grid based on a directional move and the result of an environmental scan
     */
    public void Move(Vector2 dir, int[][] localMap, float inverseMoveSpeed)
    {
        if (dir.magnitude > 1 || localMap.Length <= 0 || localMap[0].Length <= 0 || localMap.Length != localMap[0].Length) return;

        //Dynamic Reallocation
        if (dir.x > 0) //RIGHT
        {
            if (3 + GetConversionFactor().x == width)
            {
                //Add Column to the right
                for (int i = 0; i < grid.Count; i++)
                {
                    grid[i].Add((int)MappingIDs.Undefined);
                }
            }
            currentLocation.x += 1;
        }
        else if (dir.x < 0) //LEFT
        {
            if (GetConversionFactor().x == 0)
            {
                //Add Column to the left
                for (int i = 0; i < grid.Count; i++)
                {
                    grid[i].Insert(0, (int)MappingIDs.Undefined);
                }
                localOrigin.x += 1;
                gridCorner[0] -= 1;
            }
            currentLocation.x -= 1;
        }

        if (dir.y > 0) //Up
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
            }
            currentLocation.y -= 1;
        }
        else if (dir.y < 0) //Down
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
            }
            currentLocation.y += 1;
        }
        width = grid[0].Count;
        height = grid.Count;
        SetLocalValues(localMap);

        //DEBUG Unit Tests
        //Print(inverseMoveSpeed);
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
                grid[i + (int)GetConversionFactor().y][j + (int)GetConversionFactor().x] = localMap[i][j];
            }
        }
    }

    /**
     * #### Vector2 GetConversionFactor()
     * Returns the x and y conversion values to go from array index to object space
     */
    public Vector2 GetConversionFactor() 
    {
        return new Vector2((int)currentLocation.x + (int)localOrigin.x, (int)currentLocation.y + (int)localOrigin.y); 
    }
    
    public MappingIDs GetMapping(int x, int y)
    {
        return (MappingIDs)grid[grid.Count - 1 - y][x];
    }

    /**
     * #### void Print()
     * Debugging method which prints a grid representation to console
     */
    public void Print(float inverseMoveSpeed)
    {
        string toPrint = "";
        for (int i = 0; i < grid.Count; i++)
        {
            for (int j = 0; j < grid[0].Count; j++)
            {
                toPrint += grid[i][j] + " ";
            }
            toPrint += "\n";
        }
        Debug.Log(toPrint);
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
        }
    }

    /**
     * #### void Start()
     * Unity event which runs at initialization.
     * Initializes a new coordinate grid with dimensions 3x3
     */
    private void Start()
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
    }
}
