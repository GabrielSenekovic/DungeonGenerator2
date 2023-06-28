using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlot : InventorySlot, IPointerClickHandler
{
    Image myImage;
    public EquipState state = EquipState.NONE;
    public SelectState selectState = SelectState.NONE;
    public Equipment equipment;

    private void Awake()
    {
        myImage = GetComponent<Image>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (state == EquipState.EQUIPPED || state == EquipState.SKILL || state == EquipState.NONE)
            {
                //For selecting a skill
                selectState = SelectState.WAITING;
            }
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (state == EquipState.EQUIPPED)
            {
                //For unequipping skills
                state = EquipState.WAITING;
            }
        }
    }
    public void UnEquip()
    {
        GetComponent<Image>().color = Color.white;
        state = EquipState.SKILL;
    }

    public void RefreshImage()
    {
        if (equipment && equipment.icon)
        {
            myImage.sprite = equipment.icon;
        }
        else
        {
            myImage.sprite = null;
        }
    }

    public Sprite GetIcon()
    {
        if (equipment && equipment.icon)
        {
            return equipment.icon;
        }
        return null;
    }
    public void SetImage(Sprite sprite)
    {
        myImage.sprite = sprite;
    }
    public void SetColor(Color color)
    {
        myImage.color = color;
    }
}
