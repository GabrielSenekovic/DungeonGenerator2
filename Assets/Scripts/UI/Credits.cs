using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credits : MonoBehaviour
{
    string credits;

    private void Awake() 
    {
        credits = "Product Owner - Gabriel Senekovic " +
                  "Lead Programmer - Gabriel Senekovic " +
                  "Server Assistance - Tom D Idril " +
                  "Web Designer and Literary Consultant - Toni Al-Nawasreh " +
                  "Programming Consultant - Clément Pirelli " + 
                  "Math Consultant - Casper Gustafsson";
    }
}
