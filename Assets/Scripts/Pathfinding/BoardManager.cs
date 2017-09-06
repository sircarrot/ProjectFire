using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

    //Both numX and numY is columns and rows count
    public int numX;
    public int numY;

    //TEST WHICH IS FASTER FOR PATHFINDING, USING GRIDS OR JUST GAMEOBJECTS
    public TileNode[,] TileGrid;
    //GameObject[,] TileObjectGrid

    void Awake()
    {
        // Automate the numX and numY, may be deleted to cut time
        foreach (Transform child in transform)
        {
            int x = Mathf.RoundToInt(child.transform.position.x);
            int y = Mathf.RoundToInt(child.transform.position.y);

            if (x > numX) numX = x;
            if (y > numY) numY = y;
        }
        numX++;
        numY++;
    }

    // Use this for initialization
    void Start () {
        InitializeGrid();
	}

    private void InitializeGrid()
    {
        Debug.Log("Initialize grid " + Time.realtimeSinceStartup);
        TileGrid = new TileNode[numX, numY]; 
        //TileObjectGrid = new GameObject[numX, numY];
        foreach (Transform child in transform)
        {
            if (child.gameObject.layer != LayerMask.NameToLayer("Tile"))
            {
                continue;
            }


            int x = Mathf.RoundToInt(child.transform.position.x);
            int y = Mathf.RoundToInt(child.transform.position.y);

            //Test to use grid or gameobjects
            TileGrid[x, y] = new TileNode(child.GetComponent<TerrainScript>().movementCost, child.position, child.GetComponent<TerrainScript>().occupiedBy);
            //TileObjectGrid[x, y] = child.gameObject;
        }
        Debug.Log("Finish grid " + Time.realtimeSinceStartup);
    }

    public List<TileNode> GetNeighbours(TileNode node)
    {
        List<TileNode> neighbours = new List<TileNode>();

        if (node.x + 1 < numX)  { neighbours.Add(TileGrid[node.x + 1, node.y]); }
        if (node.x - 1 >= 0)    { neighbours.Add(TileGrid[node.x - 1, node.y]); }
        if (node.y + 1 < numY)  { neighbours.Add(TileGrid[node.x, node.y +  1]); }
        if (node.y - 1 >= 0)    { neighbours.Add(TileGrid[node.x, node.y - 1]); }

        return neighbours;
    }

    public TileNode PosToGrid(Vector3 vector)
    {
        return TileGrid[Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y)];
    }

    public void moveOccupy(Vector3 initPos, Vector3 endPos)
    {
        TileNode temp = PosToGrid(initPos);
        temp.occupiedBy = 0;
        temp = PosToGrid(endPos);
        temp.occupiedBy = GameManager.instance.currentPlayer;

    }

}
