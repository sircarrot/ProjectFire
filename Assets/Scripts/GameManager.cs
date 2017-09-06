using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public int numberOfPlayers;
    public int currentPlayer;

    public Transform UnitList;
    public static GameManager instance = null;

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

    // Use this for initialization
    void Start () {
        currentPlayer = 1;
	}

    public void endTurn()
    {
        Debug.Log("Next turn");
        foreach(Transform child in UnitList)
        {
            child.GetComponent<Units>().onSwitchTurn();
        }

        currentPlayer++;
        if (currentPlayer > numberOfPlayers) currentPlayer = 1;
    }

}
