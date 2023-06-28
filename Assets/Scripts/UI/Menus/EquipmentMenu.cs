using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentMenu : MonoBehaviour
{
    public List<EquipmentModel> players;

    public int currentPlayer;

    public EquipmentSlot leftHand;
    public EquipmentSlot rightHand;

    public List<EquipmentSlot> inventorySlots; //The skills in the list of skills

    [SerializeField] Transform inventoryGrid;

    [SerializeField] EquipmentSlot skillSlotPrefab;

    [SerializeField] Sprite emptySlot;

    public SelectionData selection;
    [SerializeField] Inventory inventory;

    //There are 4 skills that can be switched between
    //There are also a list of all viable skills. When one skill is equipped, it is then darkened in the list
    //Therefore, somehow we need to keep track of what skills are currently equipped
    //When a skill is equipped from the list, darken the skill in the list and disable it
    //When a skill is dequipped, find it in the list and reable it
    //Get a sprite for an unequipped slot

    private void Start()
    {
        selection = new SelectionData(-1, false);

        OpenMenu();

        for (int i = 0; i < 30; i++) //normal inventory
        {
            EquipmentSlot slot = Instantiate(skillSlotPrefab, inventoryGrid);
            inventorySlots.Add(slot);
            inventorySlots[i].index += i;
            inventorySlots[i].state = EquipState.SKILL;
        }
    }
    void OpenMenu()
    {
        //Fetch weapons from inventory
    }
    void CheckIfEquipped(int index)
    {
        for (int j = 0; j < 4; j++)
        {
            if (players[currentPlayer].leftHandItem.name == (inventorySlots[index]).equipment.name)
            {
                inventorySlots[index].GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f);
                inventorySlots[index].state = EquipState.EQUIPPED_LIST;
            }
        }
    }
    private void Update()
    {
        for (int i = 0; i < inventorySlots.Count; i++) //Go through all the skills in the list of skills
        {
            if (inventorySlots[i].selectState == SelectState.WAITING) //Check if it is supposed to be selected
            {
                SelectSkill(inventorySlots[i]); //Then select it
            }
        }
    }

    void UnEquip(EquipmentSlot slot)
    {
        players[currentPlayer].leftHandItem = null;
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (slot.equipment.name == inventorySlots[i].equipment.name)
            {
                inventorySlots[i].GetComponent<Image>().color = Color.white;
                inventorySlots[i].state = EquipState.SKILL;
                slot.state = EquipState.NONE;
                slot.GetComponent<Image>().sprite = emptySlot;
            }
        }
    }

    public void SelectSkill(EquipmentSlot slot)
    {
        switch (slot.state)
        {
            case EquipState.EQUIPPED:
                //If you have selected a skill from the equipped skill
                //if (selection.selectedSkill == -1 && selection.fromList == false) { OnSelect(slot.index); return; }
                //The above checks if you have not selected anything yet
                SelectEquippedSkill(slot);
                break;
            case EquipState.SKILL:
                //If you have selected a skill from the skill list
                SelectSkillFromList(slot);
                break;
            case EquipState.NONE:
                //if (selection.selectedSkill == -1 && selection.fromList == false) { OnSelect(slot.index); return; }
                SelectEquippedSkill(slot);
                break;
        }
    }
    void SelectEquippedSkill(EquipmentSlot slot)
    {
        if (selection.fromList == false)
        {
            //If you have already selected something from your equipped skills
            /*AttackIdentifier temp = players[currentPlayer].attacks[slot.index].attack;
            players[currentPlayer].attacks[slot.index].attack = players[currentPlayer].attacks[selection.selectedSkill].attack;
            players[currentPlayer].attacks[selection.selectedSkill].attack = temp;
            slot.equipment = players[currentPlayer].attacks[slot.index].equ;
            slot.RefreshImage();

            selection.selectedSkill = -1;*/
        }
        else
        {
            //If you have already selected something in the skill list
            //Make it dark in the skill list, and make the skill from the equip bright
            //Replace the sprite and attack in the game
            inventorySlots[selection.selectedSkill].SetColor(new Color(0.4f, 0.4f, 0.4f));
            inventorySlots[selection.selectedSkill].state = EquipState.EQUIPPED_LIST;
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (slot.equipment && slot.equipment.name == inventorySlots[i].equipment.name)
                {
                    inventorySlots[i].UnEquip();
                }
            }
            players[currentPlayer].leftHandItem = inventorySlots[selection.selectedSkill].equipment;
            slot.state = EquipState.EQUIPPED;
            slot.equipment = inventorySlots[selection.selectedSkill].equipment;
            slot.SetImage(players[currentPlayer].leftHandItem.icon);
            selection.selectedSkill = -1; selection.fromList = false;
        }
    }
    void SelectSkillFromList(EquipmentSlot slot)
    {
        if (selection.selectedSkill != -1 && selection.fromList == false)
        {
            //If you already have a skill from the equipped skills selected, deselect it
        }
        else if (selection.selectedSkill != -1 && selection.fromList == true)
        {
            //If you already have a skill from the list selected, deselect it
            inventorySlots[selection.selectedSkill].UnEquip();
            inventorySlots[selection.selectedSkill].selectState = SelectState.NONE;
            selection.selectedSkill = -1;
        }
        else if (selection.selectedSkill == -1)
        {
            //If you have nothing selected, select the slot
            selection.selectedSkill = slot.index;
            selection.fromList = true;
            inventorySlots[slot.index].SetColor(Color.blue);
            inventorySlots[slot.index].selectState = SelectState.SELECTED;
        }
    }
}

