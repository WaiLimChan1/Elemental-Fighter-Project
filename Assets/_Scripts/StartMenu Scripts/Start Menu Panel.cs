using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuPanel : MonoBehaviour
{
    private StartMenuPanelHandler StartMenuPanelHandler;

    [Header("Player Information")]
    [SerializeField] private TMP_InputField EnterName;
    [SerializeField] private TMP_Dropdown ChampionSelection;

    [Header("Match Making Options")]
    [SerializeField] private Button HostButton;
    [SerializeField] private Button RoomListButton;
    [SerializeField] private Button JoinButton;

    public string GetPlayerName()
    {
        return EnterName.text;
    }

    public int GetChampionSelectionIndex()
    {
        return ChampionSelection.value;
    }

    private void ClickedHostButton()
    {
        StartMenuPanelHandler.HostPanel.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    private void ClickedRoomListButton()
    {
        StartMenuPanelHandler.RoomBrowserPanel.gameObject.SetActive(true);
        GlobalManagers.Instance.NetworkRunnerController.OnJoinLobby();
        this.gameObject.SetActive(false);
    }

    private void ClickedJoinButton()
    {
        StartMenuPanelHandler.JoinPanel.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    private void Start()
    {
        StartMenuPanelHandler = GetComponentInParent<StartMenuPanelHandler>();

        HostButton.onClick.AddListener(ClickedHostButton);
        RoomListButton.onClick.AddListener(ClickedRoomListButton);
        JoinButton.onClick.AddListener(ClickedJoinButton);
    }
}