using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePanel : MonoBehaviour
{
    private StartMenuPanelHandler StartMenuPanelHandler;

    [Header("Life Time Data")]
    [SerializeField] private TextMeshProUGUI LifeTimeKillsText;
    [SerializeField] private TextMeshProUGUI LifeTimeDamageDealtText;
    [SerializeField] private TextMeshProUGUI LifeTimeDamageTakenText;

    [Header("Last Match Data")]
    [SerializeField] private GameObject LastMatchDataSection;
    [SerializeField] private TextMeshProUGUI MatchRankingText;
    [SerializeField] private TextMeshProUGUI GamePointsText;

    [SerializeField] private TextMeshProUGUI KillsText;
    [SerializeField] private TextMeshProUGUI DamageDealtText;
    [SerializeField] private TextMeshProUGUI DamageTakenText;

    [Header("Images")]
    [SerializeField] private Image ChampionImage;
    [SerializeField] private Image[] Items;

    [SerializeField] private Sprite[] ChampionImages;

    [Header("Navigation")]
    [SerializeField] private Button BackButton;

    private void ClickedBackButton()
    {
        StartMenuPanelHandler.StartMenuPanel.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void Start()
    {
        StartMenuPanelHandler = GetComponentInParent<StartMenuPanelHandler>();
        BackButton.onClick.AddListener(ClickedBackButton);
    }

    public void GetData()
    {
        PlayFabManager.GetLocalPlayerLifeTimeData(SuccessfullyGotLifeTimeData, UnsuccessfullyGotLifeTimeData);
        PlayFabManager.GetLocalPlayerMatchData(SuccessfullyGotLastMatchData, UnsuccessfullyGotLastMatchData);
    }

    //----------------------------------------------------------------------------------------
    //Life Time Data
    public void SuccessfullyGotLifeTimeData(GetUserDataResult result)
    {
        PlayFabManager.OnLifeTimeDataRecieved(result);

        if (PlayFabManager.LifeTimeDataUsable(result))
        {
            LifeTimeKillsText.text = "Kills: " + PlayFabManager.PlayerLifeTimeData.LifeTimeKills;
            LifeTimeDamageDealtText.text = "DMG Dealt: " + (int)PlayFabManager.PlayerLifeTimeData.LifeTimeDamageDealt;
            LifeTimeDamageTakenText.text = "DMG Taken: " + (int)PlayFabManager.PlayerLifeTimeData.LifeTimeDamageTaken;
        }
        else
        {
            LifeTimeKillsText.text = "Kills: " + 0;
            LifeTimeDamageDealtText.text = "Dmg Dealt: " + 0;
            LifeTimeDamageTakenText.text = "Dmg Taken: " + 0;
        }
    }

    public void UnsuccessfullyGotLifeTimeData(PlayFabError error)
    {
        PlayFabManager.OnError(error);
    }
    //----------------------------------------------------------------------------------------



    //----------------------------------------------------------------------------------------
    //Last Match Data
    public void SuccessfullyGotLastMatchData(GetUserDataResult result)
    {
        PlayFabManager.OnMatchDataRecieved(result);

        if (PlayFabManager.MatchDataUsable(result))
        {
            LastMatchDataSection.SetActive(true);

            MatchRankingText.text = "Match Ranking: " + PlayFabManager.PlayerMatchData.MatchRanking;
            GamePointsText.text = "Game Points: " + PlayFabManager.PlayerMatchData.GamePoints;

            KillsText.text = "Kills: " + PlayFabManager.PlayerMatchData.TotalKills;
            DamageDealtText.text = "DMG Dealt: " + PlayFabManager.PlayerMatchData.TotalDamageDealt;
            DamageTakenText.text = "DMG Taken: " + PlayFabManager.PlayerMatchData.TotalDamageTaken;

            int ChampionSelectionIndex = PlayFabManager.PlayerMatchData.ChampionSelectionIndex;
            if (ChampionSelectionIndex >= 0 && ChampionSelectionIndex < ChampionImages.Length)
            {
                ChampionImage.sprite = ChampionImages[ChampionSelectionIndex];
                ChampionImage.gameObject.SetActive(true);
            }
            else
                ChampionImage.gameObject.SetActive(false);

            for (int i = 0; i < Items.Length; i++) Items[i].gameObject.SetActive(false);
            if (ItemManager.Instance != null)
            {
                string itemIndexesAsString = PlayFabManager.PlayerMatchData.ItemIndexes;
                string[] dataArray = itemIndexesAsString.Split(',');

                for (int i = 0; i < dataArray.Length && i < Items.Length; i++)
                {
                    if (int.TryParse(dataArray[i], out int value))
                    {
                        if (value >= 0 && value < ItemManager.Instance.Items.Length)
                        {
                            Items[i].gameObject.SetActive(true);
                            Items[i].sprite = ItemManager.Instance.Items[value].itemSprite;
                        }
                    }
                }
            }
        }
        else
        {
            LastMatchDataSection.SetActive(false);
        }
    }

    public void UnsuccessfullyGotLastMatchData(PlayFabError error)
    {
        PlayFabManager.OnError(error);
    }
    //----------------------------------------------------------------------------------------
}
