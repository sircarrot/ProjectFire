using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Village : InteractableObjects {

    public GameObject villageUI;

    // Use this for initialization
    void Start () {
		
	}

    public override void onSelect()
    {
        Debug.Log("village selected");
        villageUI.SetActive(true);
    }
    public override void onDeselect()
    {
        Debug.Log("village deselected");
        villageUI.SetActive(false);
    }

}
