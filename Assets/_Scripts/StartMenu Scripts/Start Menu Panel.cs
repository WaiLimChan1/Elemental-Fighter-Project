using PlayFab;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuPanel : MonoBehaviour
{
    private StartMenuPanelHandler StartMenuPanelHandler;

    [Header("Player Account Information")]
    [SerializeField] private Button ProfileButton;
    [SerializeField] private Button LogOutButton;

    [Header("Player Information")]
    [SerializeField] private TMP_InputField EnterName;

    //[SerializeField] private TMP_Dropdown ChampionSelection;
    [SerializeField] private Button ChampionSelectionButton;
    private int championSelectionIndex;

    [Header("Match Making Options")]
    [SerializeField] private Button HostButton;
    [SerializeField] private Button RoomListButton;
    [SerializeField] private Button JoinButton;

    public string GetPlayerName()
    {
        return EnterName.text;
    }

    public void SetChampionSelectionIndex(int index)
    {
        championSelectionIndex = index;
    }

    public int GetChampionSelectionIndex()
    {
        return championSelectionIndex;
    }

    private void ClickedProfileButton()
    {
        StartMenuPanelHandler.ProfilePanel.gameObject.SetActive(true);
        StartMenuPanelHandler.ProfilePanel.GetData();
        this.gameObject.SetActive(false);
    }

    private void ClickedLogOutButton()
    {
        PlayFabClientAPI.ForgetAllCredentials();
    }

    private void ClickedChampionSelect()
    {
        StartMenuPanelHandler.ChampionSelectionPanel.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
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

        ProfileButton.onClick.AddListener(ClickedProfileButton);
        LogOutButton.onClick.AddListener(ClickedLogOutButton);
        ChampionSelectionButton.onClick.AddListener(ClickedChampionSelect);
        HostButton.onClick.AddListener(ClickedHostButton);
        RoomListButton.onClick.AddListener(ClickedRoomListButton);
        JoinButton.onClick.AddListener(ClickedJoinButton);
    }
}
