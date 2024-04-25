using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [Header("GameUI Components")]
    [SerializeField] private PreRound_GameUI PreRound_GameUI;
    [SerializeField] private Round_GameUI Round_GameUI;
    [SerializeField] private RoundResults_GameUI RoundResults_GameUI;

    public void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }
    
    public void InactivateAllComponents()
    {
        PreRound_GameUI.gameObject.SetActive(false);
        Round_GameUI.gameObject.SetActive(false);
        RoundResults_GameUI.gameObject.SetActive(false);
    }

    public void Update()
    {
        InactivateAllComponents();
        if (!GameManager.CanUseGameManager()) return;

        GameManager gameManager = GameManager.Instance;
        float remainingTime = Mathf.Round(gameManager.getRemainingTime());

        if (gameManager.GameStateNetworked == GameManager.GameState.PRE_ROUND)
        {
            PreRound_GameUI.gameObject.SetActive(true);
            PreRound_GameUI.UpdateGameUI(gameManager.Round, remainingTime);
        }
        else if (gameManager.GameStateNetworked == GameManager.GameState.ROUND)
        {
            Round_GameUI.gameObject.SetActive(true);
            Round_GameUI.UpdateGameUI(remainingTime);
        }
        else if (gameManager.GameStateNetworked == GameManager.GameState.ROUND_RESULTS)
        {
            RoundResults_GameUI.gameObject.SetActive(true);
            RoundResults_GameUI.UpdateGameUI(gameManager.Round, remainingTime, gameManager.RoundRankingReversedNetworked);
        }
    }
}
