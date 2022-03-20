using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterData : MonoBehaviour
{
    public string givenName;

    public string GetName()
    {
        if(givenName == "")
        {
            givenName = NameDatabase.GetRandomName();
        }
        return givenName;
    }
}