using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour {

    [HideInInspector] public BoardManager board; 
    public List<TileNode> path = new List<TileNode>();

    // Used for displayMovement
    public HashSet<TileNode> displaySet = new HashSet<TileNode>();
    public HashSet<TileNode> attackSet = new HashSet<TileNode>();

    // Use this for initialization
    void Start () {
    board = GetComponent<BoardManager>();
	}

    public void displayMovement(Vector3 startPos, int movement, int attackRange)
    {
        // Position of unit
        TileNode startNode = board.PosToGrid(startPos);

        // Nodes to process including the movement remaining upon reaching that node
        List<TileNode> openSet = new List<TileNode>();
        List<int> movementSet = new List<int>();
        List<int> attackRangeSet = new List<int>();

        // Adds initial nodes to list
        openSet.Add(startNode);
        movementSet.Add(movement);

        // Clears the display set again just in case
        displaySet.Clear();

        while(openSet.Count > 0)
        {
            // Takes in first element at list and remove the element in the list
            TileNode currentNode = openSet[0];
            int moveRemaining = movementSet[0];
            openSet.Remove(currentNode);
            movementSet.RemoveAt(0);

            // Current node is definitely reachable, so add to display list
            displaySet.Add(currentNode);

            foreach (TileNode neighbour in board.GetNeighbours(currentNode))
            {
                // If neighbouring node is already reachable, skip process
                if (displaySet.Contains(neighbour) || (neighbour.occupiedBy != 0 && neighbour.occupiedBy != GameManager.instance.currentPlayer))
                    continue;

                //Checks if movement is valid
                if (moveRemaining - neighbour.movementCost > 0)
                {
                    // If open set contains neighbour, check for efficiency of movement
                    if(openSet.Contains(neighbour))
                    {
                        // If node is already in the set but there is another efficient path to the node
                        // update the movement set to the newest movement value that is larger
                        if(movementSet[openSet.IndexOf(neighbour)] < moveRemaining - neighbour.movementCost)
                        {
                            movementSet[openSet.IndexOf(neighbour)] = moveRemaining - neighbour.movementCost;
                            Debug.Log("found another path");
                        }
                    }
                    else
                    {
                        // Otherwise if the node doesn't exist in open set then process it again
                        openSet.Add(neighbour);
                        movementSet.Add(moveRemaining - neighbour.movementCost);
                    }
                }
                // If after movement it is 0, node is reachable but no need to process that node for movement
                else if(moveRemaining - neighbour.movementCost == 0)
                {
                    displaySet.Add(neighbour);
                }

            }
        }

        // After obtaining the display set, display the results
        foreach (TileNode node in displaySet)
        {
            // Used to display the attackable tiles
            openSet.Add(node);
            attackRangeSet.Add(attackRange);

            //RaycastHit2D tile = Physics2D.Raycast(node.position, Vector2.zero, 0, 1 << LayerMask.NameToLayer("Tile"));
            RaycastHit2D tile = Physics2D.Raycast(node.position, Vector2.zero, 0, LayerMask.GetMask("Tile"));
            tile.collider.GetComponent<SpriteRenderer>().color = Color.blue;
        }

        displayAttack(openSet, attackRangeSet);
    }

    public void displayAttack(List<TileNode> openSet, List<int> attackRangeSet)
    {
        attackSet.Clear();

        // Finding the attack tiles outside of movement range
        while (openSet.Count > 0)
        {
            TileNode currentNode = openSet[0];
            int range = attackRangeSet[0];
            openSet.Remove(currentNode);
            attackRangeSet.RemoveAt(0);


            foreach (TileNode neighbour in board.GetNeighbours(currentNode))
            {
                // If neighbouring tile is attackable or moveable, skip that tile
                if (attackSet.Contains(neighbour) || displaySet.Contains(neighbour)) continue;

                attackSet.Add(neighbour);

                // Updates the attack range if already in the set
                if (openSet.Contains(neighbour))
                {
                    if (attackRangeSet[openSet.IndexOf(neighbour)] < range)
                    {
                        attackRangeSet[openSet.IndexOf(neighbour)] = range;
                        Debug.Log("found another range");
                        continue;
                    }
                }

                // For displaying ranged attacks
                if (range > 1)
                {
                    openSet.Add(neighbour);
                    attackRangeSet.Add(range-1);
                }
            }
        }

        foreach (TileNode node in attackSet)
        {
            // Bug: Tile with a unit on top will not be highlighted
            // TODO: Find a better way to do this, such as using a gameobject array and directly accessing it instead of using raycast
            RaycastHit2D tile = Physics2D.Raycast(node.position, Vector2.zero, 0, LayerMask.GetMask("Tile"));
            tile.collider.GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    public void removeDisplayMove()
    {
        // Removing the color from displayMovement()
        foreach (TileNode node in displaySet)
        {
            RaycastHit2D tile = Physics2D.Raycast(node.position, Vector2.zero, 0, LayerMask.GetMask("Tile"));
            tile.collider.GetComponent<SpriteRenderer>().color = Color.white;
        }
        foreach (TileNode node in attackSet)
        {
            RaycastHit2D tile = Physics2D.Raycast(node.position, Vector2.zero, 0, LayerMask.GetMask("Tile"));
            tile.collider.GetComponent<SpriteRenderer>().color = Color.white;
        }

        // Clears the display set
        displaySet.Clear();
        attackSet.Clear();
    }

    public void FindPath(Vector3 startPos, Vector3 endPos)
    {
        TileNode startNode = board.PosToGrid(startPos);
        TileNode endNode = board.PosToGrid(endPos);

        List<TileNode> openSet = new List<TileNode>();
        HashSet<TileNode> closedSet = new HashSet<TileNode>();

        openSet.Add(startNode);

        while(openSet.Count > 0)
        {
            TileNode currentNode = openSet[0];
            for(int i = 1; i<openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                    currentNode = openSet[i];
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == endNode)
            {
                retracePath(startNode, endNode);
                return;
            }

            foreach (TileNode neighbour in board.GetNeighbours(currentNode))
            {
                if (closedSet.Contains(neighbour))
                    continue;

                int newMovementCost = currentNode.gCost*currentNode.movementCost + GetDistance(currentNode, neighbour);
                if (newMovementCost < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCost;
                    neighbour.hCost = GetDistance(neighbour, endNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour)) openSet.Add(neighbour);
                }
            }
        }
    }

    void retracePath(TileNode startNode, TileNode endNode)
    {
        TileNode currentNode = endNode;

        path.Clear();

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        //foreach (TileNode node in path)
        //{
        //    Debug.Log(node.position);
        //}
    }

    // Returns the number of tiles needed to move from tile A to tile B
    int GetDistance(TileNode nodeA, TileNode nodeB) 
    {
        int distanceX = Mathf.Abs(nodeA.x - nodeB.x);
        int distanceY = Mathf.Abs(nodeA.y - nodeB.y);

        return (distanceX + distanceY);
    }
}
