
/**
* A collection of labels for nodes to be used in pathfinding
* Undefined: Node was created on unmapped areas of the grid
* Free: Node was created on a fully traversable area of the grid
* Full: Node was created on a fully un-traversable area of the grid
* Mixed: Node was created on an area of the grid with mixed traversable and un-traversabele space
*/
public enum NodeIDs { Undefined, Free, Full, Mixed }