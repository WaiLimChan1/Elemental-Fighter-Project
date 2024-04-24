using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class HostPanel : MonoBehaviour
{
    private StartMenuPanelHandler StartMenuPanelHandler;

    [Header("Selection Options")]
    [SerializeField] private TMP_Dropdown GameModeSelection;
    [SerializeField] private TMP_InputField EnterRoomCode;

    [Header("Navigation Options")]
    [SerializeField] private Button BackButton;
    [SerializeField] private Button ConfirmButton;

    private void ClickedBackButton()
    {
        StartMenuPanelHandler.StartMenuPanel.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    private void ClickedConfirmButton()
    {
        StartMenuPanelHandler.StartGame(GameMode.Host, EnterRoomCode.text, (NetworkRunnerController.EF_GAME_MODE) GameModeSelection.value);
        this.gameObject.SetActive(false);
    }

    private void Start()
    {
        StartMenuPanelHandler = GetComponentInParent<StartMenuPanelHandler>();

        BackButton.onClick.AddListener(ClickedBackButton);
        ConfirmButton.onClick.AddListener(ClickedConfirmButton);
    }
}
