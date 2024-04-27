using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchResults_GameUI : MonoBehaviour
{
    [Header("MatchResults_GameUI Components")]
    [SerializeField] private TextMeshProUGUI TitleText;
    [SerializeField] private RoundResultsListItem_GameUI[] MatchResultsListItems;
    [SerializeField] private Button returnToStartMenu;


    public void UpdateGameUI(bool uploadedLocalPlayerMatchData, NetworkArray<NetworkString<_16>> MatchResultNamesNetworked, NetworkArray<int> MatchResultPointsNetworked)
    {
        TitleText.text = $"Match Results";

        if (uploadedLocalPlayerMatchData) returnToStartMenu.interactable = true;
        else returnToStartMenu.interactable = false;

        foreach (RoundResultsListItem_GameUI item in MatchResultsListItems)
            item.gameObject.SetActive(false);

        for (int i = 0; i < MatchResultNamesNetworked.Length && i < MatchResultsListItems.Length; i++)
        {
            if (MatchResultNamesNetworked[i].ToString() == "") return;

            MatchResultsListItems[i].gameObject.SetActive(true);
            MatchResultsListItems[i].UpdateGameUI(i + 1, MatchResultNamesNetworked[i].ToString(), MatchResultPointsNetworked[i]);
        }
    }

    public void OnClick()
    {
        GlobalManagers.Instance.NetworkRunnerController.ShutDownRunner();
    }
}

