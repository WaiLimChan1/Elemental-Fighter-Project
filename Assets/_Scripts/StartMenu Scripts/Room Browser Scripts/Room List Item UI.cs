using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NetworkRunnerController;

public class RoomListItemUI : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI RoomCodeText;
    [SerializeField] public TextMeshProUGUI GameModeText;
    [SerializeField] public TextMeshProUGUI PlayersText;
    [SerializeField] public TextMeshProUGUI StatusText;
    [SerializeField] public Button JoinButton;

    SessionInfo sessionInfo;

    static int MaxNameLength = 10;

    public void SetInformation(SessionInfo sessionInfo)
    {
        this.sessionInfo = sessionInfo;

        //Room Code
        if (sessionInfo.Name.Length > MaxNameLength) RoomCodeText.text = sessionInfo.Name.Substring(0, MaxNameLength) + "...";
        else RoomCodeText.text = sessionInfo.Name;

        //Game Mode Text
        sessionInfo.Properties.TryGetValue("EfGameMode", out var EfGameMode);
        if (EfGameMode != null)
        {
            NetworkRunnerController.EF_GAME_MODE EFGameMode = (EF_GAME_MODE) EfGameMode.PropertyValue;
            GameModeText.text = GetEFGameModeString(EFGameMode);
        }

        //Players
        PlayersText.text = $"{sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}";

        //Status
        if (sessionInfo.IsOpen) StatusText.text = "Open";
        else StatusText.text = "Closed";

        bool isJoinButtonActive = true;
        if (sessionInfo.PlayerCount >= sessionInfo.MaxPlayers) isJoinButtonActive = false;
        if (!sessionInfo.IsOpen) isJoinButtonActive = false;

        JoinButton.gameObject.SetActive(isJoinButtonActive);
    }

    public void OnClick()
    {
        StartMenuPanelHandler.Instance.StartGame(GameMode.Client, sessionInfo.Name);
        StartMenuPanelHandler.Instance.gameObject.SetActive(false);
    }
}
