using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class ItemGenerator_Debugger : MonoBehaviour
{
    [SerializeField] Inventory inventory;
    [SerializeField] ItemGenerator itemGenerator;
    [SerializeField] FurnitureDatabase furnitureDatabase;
    [SerializeField] Equipment sword;
    [SerializeField] Sprite swordSprite;
    [SerializeField] Equipment bow;
    [SerializeField] Sprite bowSprite;

    public void FillInventoryWithRandomItems()
    {
        for (int i = 0; i < inventory.inventorySlots.Count; i++)
        {
            Sprite temp = itemGenerator.GenerateItemSprite();
            if (temp == null) { return; }
            AddItem(GenerateRandomItem(temp));
        }
    }
    public void FillInventoryWithFurniture()
    {
        for (int i = 0; i < inventory.inventorySlots.Count; i++)
        {
            Item temp = new Item();
            temp.types.Add(Item.ItemType.InventoryItem);
            FurnitureDatabase.DatabaseEntry entry = furnitureDatabase.GetDatabaseEntry(i);
            temp.sprite = entry.inventorySprite;
            temp.name = entry.name;
            temp.size = 1;
            AddItem(temp);
        }
    }
    public Item GenerateRandomItem(Sprite sprite)
    {
        Item temp = new Item();
        temp.types.Add(Item.ItemType.IngredientItem);
        temp.sprite = sprite;
        temp.size = 1;

        return temp;
    }
    public void AddItem(Item item)
    {
        for (int i = 0; i < inventory.inventorySlots.Count; i++)
        {
            if (inventory.inventorySlots_Item[i] == null)
            {
                inventory.inventorySlots_Item[i] = item;
                inventory.inventorySlots[i].GetComponentInChildren<Image>().sprite = item.sprite;
                return;
            }
        }
    }
    public void AddSword()
    {
        Item item = new Item();
        item.myObject = sword.gameObject;
        item.types.Add(Item.ItemType.WeaponItem);
        item.name = "Sword";
        item.sprite = swordSprite;
        AddItem(item);
    }
    public void AddBow()
    {
        Item item = new Item();
        item.myObject = bow.gameObject;
        item.types.Add(Item.ItemType.WeaponItem);
        item.name = "Bow";
        item.sprite = bowSprite;
        AddItem(item);
    }
}
