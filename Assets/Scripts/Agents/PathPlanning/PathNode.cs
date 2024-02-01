
/**
 * Class for storing the A* tree structure during pathfinding
 */
public class PathNode
{
    public float g;
    public float f;
    public PathNode parent;
    public QuadTreeNode node;

    public PathNode(float inG, PathNode inParent, QuadTreeNode inNode) { g = inG; parent = inParent; node = inNode; }
}
