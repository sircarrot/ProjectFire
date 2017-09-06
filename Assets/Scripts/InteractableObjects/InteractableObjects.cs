using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableObjects : MonoBehaviour {

    public int playerNumber;
    //public bool selected = false;

    public abstract void onSelect();
    public abstract void onDeselect();
}
