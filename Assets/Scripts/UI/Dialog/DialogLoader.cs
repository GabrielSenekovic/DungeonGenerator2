using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Line = Manuscript.Dialog.DialogNode.Line;
using PromptOption = Manuscript.Dialog.DialogNode.PromptOption;
using CharacterIdentity = Manuscript.Dialog.DialogNode.Line.CharacterIdentity;

public class DialogLoader : MonoBehaviour
{
    [SerializeField] LevelManager levelManager;
    [SerializeField] FurnitureDatabase furnitureDatabase;
    [SerializeField] LevelGenerator levelGenerator;
    public Manuscript.Dialog LoadDialog(CharacterData characterData)
    {
        string myName = characterData.GetName();
        Manuscript.Dialog dialog = new Manuscript.Dialog();

        switch (characterData.profession.GetProfession())
        {
            case ProfessionType.BAKER:
                dialog.Add("Welcome to the bakery, would you like some bread?", myName);
                dialog.Add("We got Brioche and Baguette!", myName);
                break;
            case ProfessionType.FARMER:
                dialog.Add("I've been working all day...", myName);
                dialog.Add("I'm so tired!", myName);
                break;
            case ProfessionType.BLACKSMITH:
                dialog.Add("Welcome to the smithy. We got weapons and armor.", myName);
                break;
            case ProfessionType.CARPENTER:
                dialog.Add("Hello! I think this village could use some more houses...", myName);
                dialog.Add("Perhaps you could use some help making furniture?", myName);
                dialog.Add("Should I build something?", myName);
                dialog.Add(new PromptOption("house", () => { levelGenerator.GenerateHouse("D[4,4]", levelManager.settlementData); }));
                dialog.Add(new PromptOption("table", () => { levelManager.currentRoom.Furnish("TableX2Y1", levelManager, furnitureDatabase); }));
                dialog.Add(new PromptOption("no", new Manuscript.Dialog.DialogNode("Ok, have a nice day!", myName)));
                break;
            case ProfessionType.JEWELER:
                dialog.Add("Welcome to the jewelry store! Maybe you want some new earrings?", myName);
                break;
            case ProfessionType.LEATHERWORKER:
                dialog.Add("Are you an adventurer looking for some light armor, perhaps?", myName);
                break;
            case ProfessionType.POTTER:
                dialog.Add("Welcome to the pottery! I can make anything porcelain!", myName);
                dialog.Add("My business has been blooming, since pots seem to keep being destroyed everywhere!", myName);
                dialog.Add("Want me to spawn a pot?", myName);
                dialog.Add(new PromptOption("yes", () => { levelManager.currentRoom.Furnish("Vase " + Random.Range(0, 5), levelManager, furnitureDatabase); }));
                dialog.Add(new PromptOption("no", new Manuscript.Dialog.DialogNode("Ok, have a nice day!", myName)));
                break;
            case ProfessionType.TAILOR:
                dialog.Add("Welcome to my store! Do you need a new shirt?", myName);
                break;
        }
        return dialog;
    }
}
