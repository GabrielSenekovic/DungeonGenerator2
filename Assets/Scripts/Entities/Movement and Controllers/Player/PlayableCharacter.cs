using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayableCharacter : MonoBehaviour
{
    PlayerData m_data;

    [SerializeField] Collider2D m_collider;

    MovementModel m_PMM;
    PlayerInteractionModel m_PIM;

    StatusConditionModel CM;

    EntityStatistics statistics;

    private void Awake()
    {
        m_PMM = GetComponent<MovementModel>();
        m_PIM = GetComponent<PlayerInteractionModel>();
        CM = GetComponent<StatusConditionModel>();
    }

    private void Start()
    {
        m_PIM.Initialize(m_collider);
    }

    public MovementModel GetPMM()
    {
        return m_PMM;
    }
    public PlayerInteractionModel GetPIM()
    {
        return m_PIM;
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
