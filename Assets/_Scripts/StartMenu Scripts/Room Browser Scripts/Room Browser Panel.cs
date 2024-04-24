using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomBrowserPanel : MonoBehaviour
{
    private StartMenuPanelHandler StartMenuPanelHandler;

    [Header("Information")]
    [SerializeField] public TextMeshProUGUI StatusInformation;

    [Header("Navigation Options")]
    [SerializeField] private Button BackButton;

    private void OnStartedJoiningLobby()
    {
        StatusInformation.gameObject.SetActive(true);
        StatusInformation.text = "Loading...";
    }

    private void OnJoinedLobbySuccessfully()
    {
        StatusInformation.gameObject.SetActive(false);
    }

    private void OnJoinedLobbyUnsuccessfully()
    {
        StatusInformation.gameObject.SetActive(true);
        StatusInformation.text = "Failed";
    }

    public void Awake()
    {
        NetworkRunnerController NRC = GlobalManagers.Instance.NetworkRunnerController;
        NRC.OnStartedJoiningLobby += OnStartedJoiningLobby;
        NRC.OnJoinedLobbySuccessfully += OnJoinedLobbySuccessfully;
        NRC.OnJoinedLobbyUnsuccessfully += OnJoinedLobbyUnsuccessfully;
    }

    private void ClickedBackButton()
    {
        StartMenuPanelHandler.StartMenuPanel.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    private void Start()
    {
        StartMenuPanelHandler = GetComponentInParent<StartMenuPanelHandler>();

        BackButton.onClick.AddListener(ClickedBackButton);
    }
}
