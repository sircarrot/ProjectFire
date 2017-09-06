using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Units : InteractableObjects {

    public float hp;
    public int attack;
    public int defend;
    public int movement;
    public int attackRange;
    // public bool counterattack;
    public bool moved;


    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void onSelect()
    {
        animator.SetTrigger("select");

        return;
    }
    public override void onDeselect()
    {
        animator.SetTrigger("deselect");
    }

    public void onEndMove()
    {
        animator.SetTrigger("switchturn");
        moved = true;
    }

    public void onSwitchTurn()
    {
        if (moved) animator.SetTrigger("switchturn");
        moved = false;
    }

    public void onMove()
    {
        
    }

    public void onSpawn(GameObject village)
    {
        Debug.Log("spawn");
        moved = true;
        playerNumber = village.GetComponent<Village>().playerNumber;
        animator.SetTrigger("spawn");
    }
}
