﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayableCharacter : MonoBehaviour
{
    [SerializeField] Collider2D m_collider;

    MovementModel movementModel;
    PlayerInteractionModel interactionModel;

    StatusConditionModel CM;

    EntityStatistics statistics;

    private void Awake()
    {
        movementModel = GetComponent<MovementModel>();
        interactionModel = GetComponent<PlayerInteractionModel>();
        CM = GetComponent<StatusConditionModel>();
    }

    private void Start()
    {
        interactionModel.Initialize(m_collider);
    }

    public MovementModel GetPMM()
    {
        return movementModel;
    }
    public PlayerInteractionModel GetPIM()
    {
        return interactionModel;
    }
    public StatusConditionModel GetStatusConditionModel()
    {
        return CM;
    }
    public EntityStatistics GetEntityStatistics()
    {
        return statistics;
    }
}
