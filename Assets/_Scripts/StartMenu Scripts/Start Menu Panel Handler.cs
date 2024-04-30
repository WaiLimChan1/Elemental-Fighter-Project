using Fusion;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static NetworkRunnerController;

public class StartMenuPanelHandler : MonoBehaviour
{
    public static StartMenuPanelHandler Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] public LoginPanel LoginPanel;
    [SerializeField] public StartMenuPanel StartMenuPanel;
    [SerializeField] public ChampionSelectionPanel ChampionSelectionPanel;
    [SerializeField] public ProfilePanel ProfilePanel;
    [SerializeField] public HostPanel HostPanel;
    [SerializeField] public RoomBrowserPanel RoomBrowserPanel;
    [SerializeField] public JoinPanel JoinPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void StartGame(GameMode mode, string roomCode, EF_GAME_MODE EF_GAME_MODE = EF_GAME_MODE.DEV_TEST)
    {
        NetworkRunnerController NRC = GlobalManagers.Instance.NetworkRunnerController;
        NRC.LocalPlayerName = StartMenuPanel.GetPlayerName();
        NRC.ChampionSelectionIndex = StartMenuPanel.GetChampionSelectionIndex();

        NRC.LocalGameMode = mode;
        NRC.RoomCode = roomCode;

        NRC.StartGame(mode, roomCode, EF_GAME_MODE);
    }

    public void Update()
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            LoginPanel.gameObject.SetActive(true);

            StartMenuPanel.gameObject.SetActive(false);
            ProfilePanel.gameObject.SetActive(false);
            HostPanel.gameObject.SetActive(false);
            RoomBrowserPanel.gameObject.SetActive(false);
            JoinPanel.gameObject.SetActive(false);
        }
    }
}
