using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour, IMenu
{
    public enum InventoryState
    {
        All = 0,
        Items = 1,
        Weaponry = 2,
        Armory = 3,
        Accesories = 4,
        Key_Items = 5
    }
    [SerializeField] InventoryState state;
    bool list = false;
    [SerializeField] Sprite emptySlot;
    [SerializeField] Transform inventoryGrid;
    [SerializeField] Transform hotbarGrid;
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public List<Item> inventorySlots_Item = new List<Item>();
    //Either show them as a continual list with a weight value, or visually as differently sized boxes

    InventorySlot inventorySlotPrefab;
    [SerializeField] FurnitureDatabase furnitureDatabase;
    CanvasGroup canvasGroup;

    int selectedSlot = -1;

    void Awake()
    {
        inventorySlotPrefab = Resources.Load<InventorySlot>("Inventory Slot");
        inventorySlots_Item.Add(null);
        for (int i = 0; i < 30; i++) //normal inventory
        {
            InventorySlot slot = Instantiate(inventorySlotPrefab, inventoryGrid);
            slot.gameObject.GetComponent<Button>().onClick.AddListener(() => SelectItem(slot));
            inventorySlots.Add(slot);
            inventorySlots[i].index += i;
            inventorySlots_Item.Add(null);
        }
        for(int i = 0; i < 10; i++) //The hotbar inventory
        {
            InventorySlot slot = Instantiate(inventorySlotPrefab, hotbarGrid);
            slot.gameObject.GetComponent<Button>().onClick.AddListener(() => SelectItem(slot));
            inventorySlots.Add(slot);
            inventorySlots[30 + i].index += i + 30;
            inventorySlots_Item.Add(null);
        }
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SelectItem(InventorySlot slot)
    {
        if(selectedSlot == -1) { selectedSlot = slot.index; return; }
        else
        {
            Item temp = inventorySlots_Item[slot.index];
            inventorySlots_Item[slot.index] = inventorySlots_Item[selectedSlot];
            inventorySlots_Item[selectedSlot] = temp;
        }
    }
    public List<Item> FetchAllItemsOfType(Item.ItemType type)
    {
        List<Item> items = new List<Item>();
        for(int i = 0; i < inventorySlots_Item.Count; i++)
        {
            if(inventorySlots_Item[i] != null && inventorySlots_Item[i].types.Contains(type))
            {
                items.Add(inventorySlots_Item[i]);
            }
        }
        return items;
    }
    public void ChangeState(int value)
    {
        state = (InventoryState)(value % 6);
    }
    //Compressing only works in not-list mode
    public void Compress()
    {
        //Pushes items together so you can visually "free up space"
    }
    //Sorting only works in list mode
    public void SortByName()
    {

    }
    public void SortByTypeAndName()
    {
        //Sort by type, and then within those types, sort by name
    }

    public void OnOpen()
    {
    }

    public void OnClose()
    {
    }

    public CanvasGroup GetCanvas()
    {
        return canvasGroup;
    }
}
