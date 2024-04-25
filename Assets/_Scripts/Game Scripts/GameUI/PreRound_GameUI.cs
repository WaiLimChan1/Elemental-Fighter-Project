using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PreRound_GameUI : MonoBehaviour
{
    [Header("PreRound_GameUI Components")]
    [SerializeField] private TextMeshProUGUI RoundNumberText;
    [SerializeField] private TextMeshProUGUI FightStartingInText;

    public void UpdateGameUI(int roundNum, float fightStartingInTime)
    {
        RoundNumberText.text = "Round " + roundNum;
        FightStartingInText.text = "Fight Starting In " + fightStartingInTime + "s";
    }
}
