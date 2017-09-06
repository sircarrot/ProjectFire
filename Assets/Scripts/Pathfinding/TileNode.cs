using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileNode {

    public TileNode parent;
    public int movementCost;
    public bool occupied;
    public int occupiedBy;

    public int gCost;
    public int hCost;

    public Vector3 position;
    public int x;
    public int y;

    public TileNode(int _cost, Vector3 _position, int _occupiedBy){
        movementCost = _cost;
        position = _position;
        occupiedBy = _occupiedBy;
        x = Mathf.RoundToInt(_position.x);
        y = Mathf.RoundToInt(_position.y);
    }

    public int fCost{
        get
        {
            return gCost + hCost;
        }
    }
}
