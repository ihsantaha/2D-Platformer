using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    private Inventory inventory;
    public GameObject item;
    public string Name;

    private void Update()
    {
        if(IsNearPlayer())
        {     
            inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Inventory>();
            ItemAdd();
        }
    }

    bool IsNearPlayer()
    {
        RaycastHit2D hitPlayerRight = Physics2D.Raycast(transform.position, Vector2.right, 0.55f, 1 << 10);
        RaycastHit2D hitPlayerLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.55f, 1 << 10);
        return (hitPlayerRight.collider != null && hitPlayerLeft.collider != null) ? true : false;
    }

    void ItemAdd()
    {
        for (int i = 0; i < inventory.slots.Length; i++)
        {
            if (inventory.isFull[i] == false)
            {
                Destroy(this.gameObject);
                GameObject inventoryItem = Instantiate(item, inventory.slots[i].transform, false);
                inventoryItem.name = item.name;
                inventory.slots[i] = inventoryItem;
                inventory.isFull[i] = true;
                break;
            }
        }
    }

 
}
