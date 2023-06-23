using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCGenerator_Debugger : MonoBehaviour
{
    [SerializeField] GameObject NPCPrefab;
    List<NPCInteraction> NPCinteractions = new List<NPCInteraction>();
    [SerializeField] DialogManager dialogManager;
    [SerializeField] ProfessionType[] professions = new ProfessionType[] { };
    [SerializeField] Color[] debugColors = new Color[]{};
    [SerializeField] DialogLoader dialogLoader;

    private void Awake()
    {
        
    }
    public void GenerateProfessions()
    {
        for (int i = 0; i < debugColors.Length; i++)
        {
            GameObject NPC = Instantiate(NPCPrefab, new Vector2(2  + i, 2), Quaternion.identity, null);
            NPCinteractions.Add(NPC.GetComponent<NPCInteraction>());
            CharacterData data = NPC.GetComponent<NPCController>().GetData();
            data.profession.SwitchProfession(professions[i]);
            NPCinteractions[i].Initialize(dialogManager, dialogLoader.LoadDialog(data));
            MeshRenderer renderer = NPC.GetComponentInChildren<MeshRenderer>();
            Material NPCMaterial = renderer.sharedMaterial;
            Material professionMaterial = new Material(NPCMaterial);
            professionMaterial.SetColor("_BaseColor", debugColors[i]);
            renderer.sharedMaterial = professionMaterial;
        }
    }
}
