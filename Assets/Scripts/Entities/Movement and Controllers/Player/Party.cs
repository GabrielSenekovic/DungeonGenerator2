using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Party : MonoBehaviour
{
    public static Party instance;
    List<PlayableCharacter> partyMembers = new List<PlayableCharacter>();
    [SerializeField]PlayableCharacter partyLeader; //Currently played by player

    public Inventory inventory;

    float movementSpeed = 0.1f;

    public uint keys;

    private void Awake() 
    {
        if(instance == null)
        {
            instance = this;
            keys = 0;
            partyMembers.Add(partyLeader);
            for(int i = 0; i < partyMembers.Count; i++)
            {
                partyMembers[i].GetComponent<HealthModel>().onDeath += OnDeath;
            }
        }
        else
        {
            Destroy(this);
        }
    }

    public void ChangePartyLeader(int index)
    {
        partyLeader = partyMembers[index];
    }

    public PlayableCharacter GetPartyLeader()
    {
        return partyLeader;
    }

    public static void AddKey()
    {
        instance.keys++;
        UIManager.Instance.keys.Increment();
    }
    public static bool Unlock()
    {
        if(instance.keys > 0)
        {
            instance.keys--;
            return true;
        }
        return false;
    }
    public void OnDeath(GameObject entity)
    {
        if(entity == partyLeader.gameObject)
        {
            if(!ChangePartyLeader())
            {
                TotalPartyKill();
            }
        }
    }
    public bool ChangePartyLeader()
    {
        for(int i = 0; i < partyMembers.Count; i++)
        {
            if(!partyMembers[i].GetComponent<HealthModel>().isDead())
            {
                partyLeader = partyMembers[i];
                return true;
            }
        }
        return false;
    }
    void TotalPartyKill()
    {
        SceneManager.LoadSceneAsync("HQ");
    }
}
