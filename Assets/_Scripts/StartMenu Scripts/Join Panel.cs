using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinPanel : MonoBehaviour
{
    private StartMenuPanelHandler StartMenuPanelHandler;

    [Header("Join Via Room Code")]
    [SerializeField] private TMP_InputField EnterRoomCode;
    [SerializeField] private Button JoinRoomCodeButton;

    [Header("Join Random Room")]
    [SerializeField] private Button JoinRandomButton;

    [Header("Navigation Options")]
    [SerializeField] private Button BackButton;

    private void ClickJoinRoomCodeButton()
    {
        StartMenuPanelHandler.StartGame(GameMode.AutoHostOrClient, EnterRoomCode.text);
        this.gameObject.SetActive(false);
    }

    private void ClickedJoinRandomButton()
    {
        StartMenuPanelHandler.StartGame(GameMode.AutoHostOrClient, string.Empty, NetworkRunnerController.EF_GAME_MODE.ARENA);
        this.gameObject.SetActive(false);
    }

    private void ClickedBackButton()
    {
        StartMenuPanelHandler.StartMenuPanel.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    private void Start()
    {
        StartMenuPanelHandler = GetComponentInParent<StartMenuPanelHandler>();

        JoinRoomCodeButton.onClick.AddListener(ClickJoinRoomCodeButton);
        JoinRandomButton.onClick.AddListener(ClickedJoinRandomButton);
        BackButton.onClick.AddListener(ClickedBackButton);
    }
}
