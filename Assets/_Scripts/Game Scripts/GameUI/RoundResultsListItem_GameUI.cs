using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundResultsListItem_GameUI : MonoBehaviour
{
    [Header("RoundResultsListItem_GameUI Components")]
    [SerializeField] private TextMeshProUGUI PlacementText;
    [SerializeField] private TextMeshProUGUI PlayerName;
    [SerializeField] private TextMeshProUGUI PlayerPoints;

    public void UpdateGameUI(int placement, string playerName, float playerGamePoints)
    {
        PlacementText.text = placement + ")";
        PlayerName.text = playerName;
        PlayerPoints.text = "Points: " + Mathf.Round(playerGamePoints);
    }
}
