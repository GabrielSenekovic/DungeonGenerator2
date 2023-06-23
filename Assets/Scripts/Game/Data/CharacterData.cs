using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class CharacterData
{
    public string givenName;
    public Profession profession;

    public CharacterData(string givenName, Profession profession = null)
    {
        this.givenName = givenName;
        this.profession = profession;
    }

    public string GetName()
    {
        if(givenName == "")
        {
            givenName = NameDatabase.GetRandomName();
        }
        return givenName;
    }
}