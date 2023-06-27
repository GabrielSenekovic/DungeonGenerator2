using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentMenu : MonoBehaviour
{
    public List<PlayerAttackModel> players;

    public int currentPlayer;

    public SkillSlot leftHand;
    public SkillSlot rightHand;

    public List<SkillSlot> inventorySlots; //The skills in the list of skills

    [SerializeField] Transform inventoryGrid;

    [SerializeField] SkillSlot skillSlotPrefab;

    [SerializeField] Sprite emptySlot;

    public SelectionData selection;

    //There are 4 skills that can be switched between
    //There are also a list of all viable skills. When one skill is equipped, it is then darkened in the list
    //Therefore, somehow we need to keep track of what skills are currently equipped
    //When a skill is equipped from the list, darken the skill in the list and disable it
    //When a skill is dequipped, find it in the list and reable it
    //Get a sprite for an unequipped slot

    private void Start()
    {
        selection = new SelectionData(-1, false);

        for (int i = 0; i < 30; i++) //normal inventory
        {
            SkillSlot slot = Instantiate(skillSlotPrefab, inventoryGrid);
            //slot.gameObject.GetComponent<Button>().onClick.AddListener(() => { });
            inventorySlots.Add(slot);
            inventorySlots[i].index += i;
           // inventorySlots_Item.Add(null);
        }
    }
    void CheckIfEquipped(int index)
    {
        for (int j = 0; j < 4; j++)
        {
            if (players[currentPlayer].attacks[j].attack.name == (inventorySlots[index] as SkillSlot).attack.name)
            {
                inventorySlots[index].GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f);
                inventorySlots[index].state = SkillSlot.EquipState.EQUIPPED_LIST;
            }
        }
    }
    private void Update()
    {

        for (int i = 0; i < inventorySlots.Count; i++) //Go through all the skills in the list of skills
        {
            if (inventorySlots[i].selectState == SkillSlot.SelectState.WAITING) //Check if it is supposed to be selected
            {
                SelectSkill(inventorySlots[i]); //Then select it
            }
        }
    }

    void UnEquip(SkillSlot slot)
    {
        players[currentPlayer].attacks[slot.index].attack = null;
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (slot.attack.name == inventorySlots[i].attack.name)
            {
                inventorySlots[i].GetComponent<Image>().color = Color.white;
                inventorySlots[i].state = SkillSlot.EquipState.SKILL;
                slot.state = SkillSlot.EquipState.NONE;
                slot.GetComponent<Image>().sprite = emptySlot;
            }
        }
    }

    public void SelectSkill(SkillSlot slot)
    {
        switch (slot.state)
        {
            case SkillSlot.EquipState.EQUIPPED:
                //If you have selected a skill from the equipped skill
                //if (selection.selectedSkill == -1 && selection.fromList == false) { OnSelect(slot.index); return; }
                //The above checks if you have not selected anything yet
                SelectEquippedSkill(slot);
                break;
            case SkillSlot.EquipState.SKILL:
                //If you have selected a skill from the skill list
                SelectSkillFromList(slot);
                break;
            case SkillSlot.EquipState.NONE:
                //if (selection.selectedSkill == -1 && selection.fromList == false) { OnSelect(slot.index); return; }
                SelectEquippedSkill(slot);
                break;
        }
    }
    void SelectEquippedSkill(SkillSlot slot)
    {
        if (selection.fromList == false)
        {
            //If you have already selected something from your equipped skills
            AttackIdentifier temp = players[currentPlayer].attacks[slot.index].attack;
            players[currentPlayer].attacks[slot.index].attack = players[currentPlayer].attacks[selection.selectedSkill].attack;
            players[currentPlayer].attacks[selection.selectedSkill].attack = temp;
            slot.attack = players[currentPlayer].attacks[slot.index].attack;
            slot.RefreshImage();

            selection.selectedSkill = -1;
        }
        else
        {
            //If you have already selected something in the skill list
            //Make it dark in the skill list, and make the skill from the equip bright
            //Replace the sprite and attack in the game
            inventorySlots[selection.selectedSkill].SetColor(new Color(0.4f, 0.4f, 0.4f));
            inventorySlots[selection.selectedSkill].state = SkillSlot.EquipState.EQUIPPED_LIST;
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (slot.attack && slot.attack.name == inventorySlots[i].attack.name)
                {
                    inventorySlots[i].UnEquip();
                }
            }
            players[currentPlayer].attacks[slot.index].attack = inventorySlots[selection.selectedSkill].attack;
            slot.state = SkillSlot.EquipState.EQUIPPED;
            slot.attack = inventorySlots[selection.selectedSkill].attack;
            slot.SetImage(players[currentPlayer].attacks[slot.index].attack.icon);
            selection.selectedSkill = -1; selection.fromList = false;
        }
    }
    void SelectSkillFromList(SkillSlot slot)
    {
        if (selection.selectedSkill != -1 && selection.fromList == false)
        {
            //If you already have a skill from the equipped skills selected, deselect it
        }
        else if (selection.selectedSkill != -1 && selection.fromList == true)
        {
            //If you already have a skill from the list selected, deselect it
            inventorySlots[selection.selectedSkill].UnEquip();
            inventorySlots[selection.selectedSkill].selectState = SkillSlot.SelectState.NONE;
            selection.selectedSkill = -1;
        }
        else if (selection.selectedSkill == -1)
        {
            //If you have nothing selected, select the slot
            selection.selectedSkill = slot.index;
            selection.fromList = true;
            inventorySlots[slot.index].SetColor(Color.blue);
            inventorySlots[slot.index].selectState = SkillSlot.SelectState.SELECTED;
        }
    }
}

