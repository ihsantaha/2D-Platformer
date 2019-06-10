using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public bool[] isFull;
    public GameObject[] slots;


    public void useItem(string itemName)
    {
        bool itemFound = false;
        for(int i = 0; i < slots.Length; i++)
        {
            if(slots[i].name == itemName)
            {
                Destroy(slots[i]);
                itemFound = true;
                slots[i] = null;
                isFull[i] = false;
                Debug.Log("Item Used");
            }
        }
    }
}

