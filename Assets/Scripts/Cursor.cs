using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour {

    public GameObject selectedObject;
    public float moveTime = 0.05f;
    public float fastMoveTime = 0.03f;
    public static Cursor instance = null;
    public bool moving;

    //Tentative variables
    public Text TileInfo;
    public PathFinding pathfind;
    public int selectState;
    public bool disableCursor;

    private Vector3 tempUnitPos;

    private float startMove;
    private float inverseMoveTime;
    private Vector3 mouseLastPos;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        selectState = 0;
        moving = false;
        disableCursor = false;
        startMove = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        // When cursor is in the middle of moving, does not accept any input
        if (moving || disableCursor) return;

        int horizontal = 0;
        int vertical = 0;

        // Raw inputs
        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        // If any arrow input 
        if (horizontal != 0 || vertical != 0)
        {
            // Control for single input
            if (startMove == 0f)
            {
                startMove = Time.time;
                inverseMoveTime = 1f / moveTime;
                cursorMove(horizontal, vertical);
            }
            // When input is held
            if (Time.time - startMove > 0.3f)
            {
                inverseMoveTime = 1f / fastMoveTime;
                cursorMove(horizontal, vertical);
            }
        }

        // When there is no input, resets input hold timer
        if (horizontal == 0 && vertical == 0) startMove = 0f;

        // When mouse moves/click, resets cursor to mouse position
        if (isMouseMove() || Input.GetMouseButtonDown(0))
        {
            RaycastHit2D mouseTiles = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (mouseTiles.collider != null)
            {
                transform.position = mouseTiles.collider.transform.position;
                StopAllCoroutines();
                moving = false;
            }
        }

        // Finds out what is directly below the cursor
        RaycastHit2D cursor = Physics2D.Raycast(transform.position, Vector2.zero);

        TileInfo.text = cursor.collider.name;

        TileNode tempTile;

        // Select object by S key or left click
        if ((Input.GetKeyDown(KeyCode.S) || Input.GetMouseButtonDown(0)))
        {
            switch (selectState)
            {
                case 0: // If unit/village is owned by the current player
                    if (cursor.collider.tag == "Interactable" && cursor.collider.GetComponent<InteractableObjects>().playerNumber == GameManager.instance.currentPlayer)
                    {
                        // Saves selected object
                        selectedObject = cursor.collider.gameObject;
                        selectedObject.GetComponent<InteractableObjects>().onSelect();

                        // If it is a unit, display movement
                        if (cursor.collider.gameObject.layer == LayerMask.NameToLayer("Unit") && !cursor.collider.gameObject.GetComponent<Units>().moved)
                        {
                            pathfind.displayMovement(transform.position, selectedObject.GetComponent<Units>().movement, selectedObject.GetComponent<Units>().attackRange);
                            selectState = 1;
                        }
                        // If village
                        else if(cursor.collider.gameObject.layer == LayerMask.NameToLayer("Village"))
                        {
                            disableCursor = true;
                            //selectState
                            Debug.Log("Selecting Village");
                        }
                        // When it is an object but is not village or unit, output layer name
                        else
                        {
                            Debug.Log("Switch case fail, layer of object: " + LayerMask.LayerToName(cursor.collider.gameObject.layer));
                        }
                    }
                    return;

                case 1: // If unit movement is valid
                    TileNode nodeDestination = pathfind.board.TileGrid[Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)];
                    RaycastHit2D unit = Physics2D.Raycast(nodeDestination.position, Vector2.zero, 0, LayerMask.GetMask("Unit"));

                    // When clicking on a unit that is not yours (? enemy unit). Will bug if we implement ally units
                    if ((pathfind.attackSet.Contains(nodeDestination) || pathfind.displaySet.Contains(nodeDestination)) && unit.collider != null && unit.collider.GetComponent<Units>().playerNumber != selectedObject.GetComponent<Units>().playerNumber)
                    {
                        // Shows path taken, remove the displays
                        // TODO: animation of find path
                        pathfind.FindPath(selectedObject.transform.position, transform.position);
                        pathfind.removeDisplayMove();
                        tempUnitPos = selectedObject.transform.position; // Saves the unit original position
                        pathfind.path.RemoveAt(pathfind.path.Count - 1); // End node at one tile before the enemy position (best position)
                        selectedObject.transform.position = pathfind.path[pathfind.path.Count - 1].position; // Teleports the unit to the space before the attack
                        selectState = 3;
                        Debug.Log("Confirm Attack");
                    }
                    // Moving only
                    if (pathfind.displaySet.Contains(nodeDestination) && unit.collider == null)
                    {
                        Debug.Log("Movement");
                        pathfind.FindPath(selectedObject.transform.position, transform.position);
                        pathfind.removeDisplayMove();
                        tempUnitPos = selectedObject.transform.position;
                        selectedObject.transform.position = nodeDestination.position;
                        selectState = 2;
                    }
                    // Else if not within available tiles, do nothing
                    return;

                case 2:
                    // Movement case ends
                    selectedObject.GetComponent<Units>().onEndMove();
                    selectState = 0;
                    // Updating tile occupy, how about terrainscript?
                    // TODO: TerrainScript occupy, to be updated as well or not
                    pathfind.board.moveOccupy(tempUnitPos, selectedObject.transform.position);
                    return;

                case 3:
                    // Attack case ends
                    Debug.Log("Attack");
                    selectedObject.GetComponent<Units>().onEndMove();
                    selectState = 0;
                    // Updating tile occupy
                    pathfind.board.moveOccupy(tempUnitPos, selectedObject.transform.position);
                    return;
                default:
                    return;
            }

        }

        // When pressing B
        if (Input.GetKeyDown(KeyCode.B))
        {
            switch (selectState)
            {
                case 1: // If unit is selected
                    pathfind.removeDisplayMove();
                    selectedObject.GetComponent<InteractableObjects>().onDeselect();
                    selectedObject = null;
                    selectState--;
                    return;

                case 2:
                case 3: 
                    selectedObject.transform.position = tempUnitPos;
                    pathfind.displayMovement(selectedObject.transform.position, selectedObject.GetComponent<Units>().movement, selectedObject.GetComponent<Units>().attackRange);
                    selectState = 1;
                    return;

                default:
                    return;
            }
        }
    }

    public void enableCursor()
    {
        disableCursor = false;
    }

    public void spawnUnit(GameObject unitType)
    {
        // TODO: Change the parent variable, I don't think using GameObject.Find is good.
        GameObject parent = GameObject.Find("Units");
        GameObject unitSpawn = Instantiate(unitType, selectedObject.transform.position, Quaternion.identity, parent.transform);
        unitSpawn.GetComponent<Units>().onSpawn(selectedObject);
    }

    // Movement using arrow buttons, limit by border
    private void cursorMove(int xDir, int yDir)
    {
        // Start of cursor
        Vector2 start = transform.position;

        // Detect for end of board
        if (start.x + xDir < 0 || start.x + xDir > pathfind.board.numX) xDir = 0;
        if (start.y + yDir < 0 || start.y + yDir > pathfind.board.numY) yDir = 0;

        // Move to end
        Vector2 end = start + new Vector2(xDir, yDir);

        StartCoroutine(SmoothMovement(new Vector3(xDir, yDir, 0), end));
    }

    // Smooth movement of cursor using arrow pad
    private IEnumerator SmoothMovement(Vector3 dir, Vector3 end)
    {
        if (!moving && !isMouseMove())
        {
            moving = true;

            float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            while (sqrRemainingDistance > float.Epsilon)
            {
                Vector3 newPosition = Vector3.MoveTowards(transform.position, end, inverseMoveTime * Time.deltaTime);
                transform.position = newPosition;
                sqrRemainingDistance = (transform.position - end).sqrMagnitude;
                yield return null;
            }

            moving = false;
        }
    }
    
    public bool isMouseMove()
    {
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            return true;
        else
            return false;
    }
}
