using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A node representing one quad in the QuadTree. Used for pathfinding
 */
public class QuadTreeNode
{
    public float x;
    public float y;
    public float w;
    public float h;

    public QuadTreeNode NW = null;
    public QuadTreeNode NE = null;
    public QuadTreeNode SW = null;
    public QuadTreeNode SE = null;

    public QuadTreeNode parent = null;

    public int offset = 0;

    public int depth = 0;

    public Color colorOverride = Color.clear;
    public float yOverride = 0;

    public NodeIDs nodeType = NodeIDs.Undefined;
    public int nodeLoc = -1;

    public int first = -1;

    public QuadTree tree;

    /**
     * #### QuadTreeNode(float, float, float, float)
     * Constructor which takes in x, y location of the bottom left of the quad as well as width, height of the quad
     */
    public QuadTreeNode(float x, float y, float width, float height)
    {
        this.x = x;
        this.y = y;
        this.w = width;
        this.h = height;
    }

    public QuadTreeNode GetNode(Vector2 location)
    {
        if (location.x >= x && location.x < x + w
            && location.y >= y && location.y < y + h)
        {
            if (!(SE == null && SW == null && NE == null && NW == null))
            {
                QuadTreeNode NWN = NW.GetNode(location);
                QuadTreeNode NEN = NE.GetNode(location);
                QuadTreeNode SWN = SW.GetNode(location);
                QuadTreeNode SEN = SE.GetNode(location);
                if (NWN != null) return NWN;
                if (NEN != null) return NEN;
                if (SWN != null) return SWN;
                if (SEN != null) return SEN;
            }
            else
            {
                return this;
            }
        }
        return null;
    }

    public bool CheckIfWithin(Vector2 point)
    {
        return point.x >= x && point.x <= x + w && point.y >= y && point.y <= y + h;
    }

    public bool NodeIsLeaf()
    {
        return NW == null && NE == null && SE == null && NW == null;
    }

    public List<NodeDepth> GetChildren(int depth = 0)
    {
        var arr = new List<NodeDepth>();
        if (NodeIsLeaf())
        {
            if (depth == 0) arr.Add(new NodeDepth(this, depth));
            return arr;
        }
        if (!NE.NodeIsLeaf()) arr.AddRange(NE.GetChildren(depth + 1));
        else arr.Add(new NodeDepth(NE, depth + 1));
        if (!NW.NodeIsLeaf()) arr.AddRange(NW.GetChildren(depth + 1));
        else arr.Add(new NodeDepth(NW, depth + 1));
        if (!SE.NodeIsLeaf()) arr.AddRange(SE.GetChildren(depth + 1));
        else arr.Add(new NodeDepth(SE, depth + 1));
        if (!SW.NodeIsLeaf()) arr.AddRange(SW.GetChildren(depth + 1));
        else arr.Add(new NodeDepth(SW, depth + 1));
        return arr;
    }

    public List<NodeDepth> GetFreeChildren(int depth = 0)
    {
        List<NodeDepth> children = GetChildren(depth);
        List<NodeDepth> arr = new List<NodeDepth>();
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].node.nodeType == NodeIDs.Free) arr.Add(children[i]);
        }
        return arr;
    }

    public List<QuadTreeNode> GetDirections()
    {
        List<QuadTreeNode> arr = new List<QuadTreeNode>();
        NodeDepth largestRight = GetRight();
        if (largestRight.node == null) largestRight = new NodeDepth();
        else if (largestRight.node.NodeIsLeaf()) arr.Add(largestRight.node);
        else
        {
            arr.AddRange(GetSidedChildren(this, largestRight, Side.W));
        }
        
        NodeDepth largestLeft = GetLeft();
        if (largestLeft.node == null) largestLeft = new NodeDepth();
        else if (largestLeft.node.NodeIsLeaf()) arr.Add(largestLeft.node);
        else
        {
            arr.AddRange(GetSidedChildren(this, largestLeft, Side.E));
        }
        
        NodeDepth largestUp = GetUp();
        if (largestUp.node == null) largestUp = new NodeDepth();
        else if (largestUp.node.NodeIsLeaf()) arr.Add(largestUp.node);
        else
        {
            arr.AddRange(GetSidedChildren(this, largestUp, Side.S));
        }

        NodeDepth largestDown = GetDown();
        if (largestDown.node == null) largestDown = new NodeDepth();
        else if (largestDown.node.NodeIsLeaf()) arr.Add(largestDown.node);
        else
        {
            arr.AddRange(GetSidedChildren(this, largestDown, Side.N));
        }
        
        return arr;
    }

    private List<QuadTreeNode> GetSidedChildren(QuadTreeNode node, NodeDepth otherNode, Side side)
    {
        List<QuadTreeNode> arr = new List<QuadTreeNode>();
        List<NodeDepth> children = otherNode.node.GetChildren();
        for (int i = 0; i < children.Count; i++)
        {
            QuadTreeNode newNode = node;
            QuadTreeNode Adj = children[i].node.GetAdjacentQuad(side);
            for (int j = (children[i].depth + 1) + otherNode.depth; j < 0; j++)
            {
                newNode = newNode.parent;
            }
            if (tree.IsChild(Adj, newNode))
            {
                arr.Add(children[i].node);
            }
        }
        return arr;
    }

    private NodeDepth GetRight(int depth = 0)
    {
        if (parent == null) return new NodeDepth(null, -1);
        else if (nodeLoc == 1) return new NodeDepth(parent.NE, depth);
        else if (nodeLoc == 3) return new NodeDepth(parent.SE, depth);
        else return parent.GetRight(depth - 1);
    }

    private NodeDepth GetLeft(int depth = 0)
    {
        if (parent == null) return new NodeDepth(null, -1);
        else if (nodeLoc == 0) return new NodeDepth(parent.NW, depth);
        else if (nodeLoc == 2) return new NodeDepth(parent.SW, depth);
        else return parent.GetLeft(depth - 1);
    }
    private NodeDepth GetUp(int depth = 0)
    {
        if (parent == null) return new NodeDepth(null, -1);
        else if (nodeLoc == 2) return new NodeDepth(parent.NE, depth);
        else if (nodeLoc == 3) return new NodeDepth(parent.NW, depth);
        else return parent.GetUp(depth - 1);
    }

    private NodeDepth GetDown(int depth = 0)
    {
        if (parent == null) return new NodeDepth(null, -1);
        else if (nodeLoc == 0) return new NodeDepth(parent.SE, depth);
        else if (nodeLoc == 1) return new NodeDepth(parent.SW, depth);
        else return parent.GetDown(depth - 1);
    }

    private QuadTreeNode GetAdjacentQuad(Side side)
    {
        QuadTreeNode node = null;
        float tolerance = (float)tree.MinSize/10;
        float up = (h / 2) + tolerance;
        float right = (w / 2) + tolerance;
        switch (side)
        {
            case Side.N:
                {
                    Vector2 center = GetCenterPoint();
                    node = tree.GetNode(new Vector2(center.x, center.y + up));
                    break;
                }
            case Side.S:
                {
                    Vector2 center = GetCenterPoint();
                    node = tree.GetNode(new Vector2(center.x, center.y - up));
                    break;
                }
            case Side.E:
                {
                    Vector2 center = GetCenterPoint();
                    node = tree.GetNode(new Vector2(center.x + right, center.y));
                    break;
                }
            case Side.W:
                {
                    Vector2 center = GetCenterPoint();
                    node = tree.GetNode(new Vector2(center.x - right, center.y));
                    break;
                }
        }
        if (node == this) Debug.Log("ERROR FINDING SAME QUAD");
        return node;
    }

    public Vector2 GetCenterPoint()
    {
        return new Vector2(x + (w/2), y + (h/2));
    }

    /**
     * #### void Print()
     * A primarily debugging method which draws the quads overlayed in 3D space for visualization
     */
    public void Print(float time)
    {
        if (NW != null) NW.Print(time);
        if (NE != null) NE.Print(time);
        if (SW != null) SW.Print(time);
        if (SE != null) SE.Print(time);
        if (SE == null && SW == null && NE == null && NW == null)
        {
            float tx = x - 0.5f;
            float ty = y - 0.5f;
            int up = 1 + (int)yOverride;

            Color quadCol;
            switch (nodeType)
            {
                case NodeIDs.Undefined:
                    quadCol = Color.gray;
                    break;
                case NodeIDs.Full:
                    quadCol = Color.red;
                    break;
                case NodeIDs.Free:
                    quadCol = Color.green;
                    break;
                default:
                    quadCol = Color.yellow;
                    break;
            }

            if (colorOverride != Color.clear) quadCol = colorOverride;

            //var time = 3f;
            Debug.DrawLine(new Vector3(tx, up, ty), new Vector3(tx + w, up, ty), quadCol, time, false);
            Debug.DrawLine(new Vector3(tx + w, up, ty), new Vector3(tx + w, up, ty + h), quadCol, time, false);
            Debug.DrawLine(new Vector3(tx + w, up, ty + h), new Vector3(tx, up, ty + h), quadCol, time, false);
            Debug.DrawLine(new Vector3(tx, up, ty + h), new Vector3(tx, up, ty), quadCol, time, false);
            yOverride = 0;
            colorOverride = Color.clear;
        }
    }
}
