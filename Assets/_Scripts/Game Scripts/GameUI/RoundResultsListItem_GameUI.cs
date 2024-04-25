using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundResultsListItem_GameUI : MonoBehaviour
{
    [Header("RoundResultsListItem_GameUI Components")]
    [SerializeField] private TextMeshProUGUI PlacementText;
    [SerializeField] private TextMeshProUGUI PlayerName;

    public void UpdateGameUI(int placement, string playerName)
    {
        PlacementText.text = placement + ")";
        PlayerName.text = playerName;
    }
}
