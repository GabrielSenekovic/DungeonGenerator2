using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class BulletinBoard : MonoBehaviour, IInteractable
{
    [SerializeField]CanvasGroup questScreen;
    int[] seeds = new int[5];
    bool active = true;
    bool isInteractable = true;
    bool isInteractedWith = false;

    Tuple<int[], int[], int[]> GenerateNewSeeds() //here temporarily
    {
        return new Tuple<int[], int[], int[]>
        (
            new int[5]{UnityEngine.Random.Range(0, int.MaxValue),UnityEngine.Random.Range(0, int.MaxValue),UnityEngine.Random.Range(0, int.MaxValue),UnityEngine.Random.Range(0, int.MaxValue),UnityEngine.Random.Range(0, int.MaxValue)}, 
            new int[5]{UnityEngine.Random.Range(0, int.MaxValue),UnityEngine.Random.Range(0, int.MaxValue),UnityEngine.Random.Range(0, int.MaxValue),UnityEngine.Random.Range(0, int.MaxValue),UnityEngine.Random.Range(0, int.MaxValue)},
            new int[5]{UnityEngine.Random.Range(0, int.MaxValue),UnityEngine.Random.Range(0, int.MaxValue),UnityEngine.Random.Range(0, int.MaxValue),UnityEngine.Random.Range(0, int.MaxValue),UnityEngine.Random.Range(0, int.MaxValue)}
        );
    }

    public void OnInteract(PlayerInteractionModel interactionModel, StatusConditionModel statusConditionModel)
    {
        if(isInteractable)
        {
            Debug.Log("Interacted with Bulletin Board");
            questScreen.GetComponent<QuestSelect>().Initialize(GenerateNewSeeds(), this);
            UIManager.OpenOrClose(questScreen);
            UIManager.ToggleHUD();
            isInteractable = false;
        }
    }
    public void OnClose()
    {
        isInteractable = true;
        UIManager.OpenOrClose(questScreen);
        UIManager.ToggleHUD();
    }

    public bool GetIsInteractable() => isInteractable;

    public void OnLeaveInteractable()
    {
        return;
    }
}
