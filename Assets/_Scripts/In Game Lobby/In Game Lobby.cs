using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Champion;
using static NetworkRunnerController;

public class InGameLobby : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkRunnerController NRC;

    [Header("InGameLobby Components")]
    [SerializeField] private LobbyPlayerListItem[] LobbyPlayerListItems;

    [SerializeField] private TextMeshProUGUI RoomCodeText;
    [SerializeField] private TextMeshProUGUI GameModeText;

    [SerializeField] private Button LeaveButton;
    [SerializeField] private Button StartGameButton;



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Items
    public const int MAX_PLAYERS = 8;

    [Networked, Capacity(MAX_PLAYERS)] public NetworkArray<int> PlayerStausListNetworked => default;
    [Networked, Capacity(MAX_PLAYERS)] public NetworkArray<NetworkString<_8>> PlayerNameListNetworked => default;

    //-1: Turn off, 0: Send name, 1: All good

    public void ClearPlayerListNetworked()
    {
        if (!Runner.IsServer) return;
        List<int> emptyList = Enumerable.Repeat(-1, MAX_PLAYERS).ToList();
        PlayerStausListNetworked.CopyFrom(emptyList, 0, emptyList.Count);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    public void PlayerJoined(PlayerRef playerRef)
    {
        if (Runner.IsServer)
        {
            PlayerStausListNetworked.Set(playerRef.PlayerId, 0);
        }
    }

    public void PlayerLeft(PlayerRef playerRef)
    {
        if (Runner.IsServer)
        {
            PlayerStausListNetworked.Set(playerRef.PlayerId, -1);
        }
    }

    [Rpc(sources: RpcSources.All, RpcTargets.StateAuthority)]
    protected void RPC_SendName(int playerID, NetworkString<_8> playerName)
    {
        PlayerNameListNetworked.Set(playerID, playerName.ToString() + " " + playerID);
        PlayerStausListNetworked.Set(playerID, 1);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    private void ClickedLeaveButton()
    {
        NRC.ShutDownRunner();
    }

    private void ClickedStartGameButton()
    {
        Runner.SessionInfo.IsOpen = false;

        Runner.SessionInfo.Properties.TryGetValue("EfGameMode", out var EfGameMode);
        if (EfGameMode != null)
        {
            NetworkRunnerController.EF_GAME_MODE EFGameMode = (EF_GAME_MODE)EfGameMode.PropertyValue;
            NRC.SetActiveScene(GetEFGameModeString(EFGameMode));
        }
    }

    public override void Spawned()
    {
        NRC = GlobalManagers.Instance.NetworkRunnerController;

        LeaveButton.onClick.AddListener(ClickedLeaveButton);

        if (Runner.IsServer)
        {
            StartGameButton.gameObject.SetActive(true);
            StartGameButton.onClick.AddListener(ClickedStartGameButton);

            ClearPlayerListNetworked();
            PlayerJoined(Runner.LocalPlayer);
        }
        else
        {
            StartGameButton.gameObject.SetActive(false);
        }
    }

    private void UpdateTextMeshProUGUI()
    {
        if (Runner == null) return;
        RoomCodeText.text = "Room Code: " + Runner.SessionInfo.Name;

        Runner.SessionInfo.Properties.TryGetValue("EfGameMode", out var EfGameMode);
        if (EfGameMode != null)
        {
            NetworkRunnerController.EF_GAME_MODE EFGameMode = (EF_GAME_MODE)EfGameMode.PropertyValue;
            GameModeText.text = "Game Mode: " + GetEFGameModeString(EFGameMode);
        }
    }

    private void UpdateLobbyPlayerList()
    {
        if (Runner == null) return;

        for (int i = 0; i < PlayerStausListNetworked.Count(); i++)
        {
            if (PlayerStausListNetworked[i] == -1)
            {
                LobbyPlayerListItems[i].gameObject.SetActive(false);
            }
            else if (PlayerStausListNetworked[i] == 0)
            {
                LobbyPlayerListItems[i].gameObject.SetActive(false);

                //If the is local player's PlayerNameListItem
                if (Runner.LocalPlayer.PlayerId == i)
                {
                    RPC_SendName(i, NRC.LocalPlayerName);
                }
                
            }
            else if (PlayerStausListNetworked[i] == 1)
            {
                LobbyPlayerListItems[i].gameObject.SetActive(true);
                LobbyPlayerListItems[i].SetName(PlayerNameListNetworked[i].ToString());
            }
        }
    }

    public void Update()
    {
        UpdateTextMeshProUGUI();

        UpdateLobbyPlayerList();
    }
}
