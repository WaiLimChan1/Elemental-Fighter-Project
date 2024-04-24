using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static NetworkRunnerController;

public class StartMenuPanelHandler : MonoBehaviour
{
    public static StartMenuPanelHandler Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] public StartMenuPanel StartMenuPanel;
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
}
