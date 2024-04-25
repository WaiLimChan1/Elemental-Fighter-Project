using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundResults_GameUI : MonoBehaviour
{
    [Header("RoundResults_GameUI Components")]
    [SerializeField] private TextMeshProUGUI TitleText;
    [SerializeField] private RoundResultsListItem_GameUI[] RoundResultsListItems;
    

    public void UpdateGameUI(int roundNum, float remainingTime, NetworkArray<NetworkedPlayer> roundRankingReversedNetworked)
    {
        TitleText.text = $"Round {roundNum} Results ({remainingTime}s)";

        foreach (RoundResultsListItem_GameUI item in RoundResultsListItems)
            item.gameObject.SetActive(false);

        int RoundResultsListItemsIndex = 0;
        for (int i = roundRankingReversedNetworked.Length - 1; i >= 0; i--)
        {
            NetworkedPlayer currentNetworkedPlayer = roundRankingReversedNetworked[i];
            if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedChampion(currentNetworkedPlayer)) continue;

            RoundResultsListItems[RoundResultsListItemsIndex].gameObject.SetActive(true);
            RoundResultsListItems[RoundResultsListItemsIndex].UpdateGameUI(RoundResultsListItemsIndex + 1, currentNetworkedPlayer.GetPlayerName());
            RoundResultsListItemsIndex++;
        }
    }
}
