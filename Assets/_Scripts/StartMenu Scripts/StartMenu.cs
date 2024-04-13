using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System; //Needed for DateTime

public class StartMenu : MonoBehaviour
{
    [SerializeField] TMP_Text Title;
    [SerializeField] private TMP_InputField EnterName;
    [SerializeField] private TMP_InputField EnterRoomCode;
    [SerializeField] private TMP_Dropdown ChampionSelection;
    [SerializeField] private Button JoinRoomButton;
    [SerializeField] private Button CreateRoomButton;
    [SerializeField] private Button JoinRandomButton;

    private const int MIN_NAME_LENGTH = 2;
    private const int MIN_ROOM_CODE_LENGTH = 2;
    private Color INVALID_COLOR = Color.red;
    private Color VALID_COLOR = Color.black;

    private void StartGame(GameMode mode, string roomCode)
    {
        NetworkRunnerController NRC = GlobalManagers.Instance.NetworkRunnerController;
        NRC.LocalPlayerName = EnterName.text;
        GlobalManagers.Instance.PlayfabManager.playerCache.username = EnterName.text; //Save player name
        NRC.RoomCode = roomCode;
        NRC.ChampionSelectionIndex = ChampionSelection.value;
        NRC.LocalGameMode = mode;

        Debug.Log($"---------------------{mode}---------------------");
        GlobalManagers.Instance.NetworkRunnerController.StartGame(mode, roomCode);
        GlobalManagers.Instance.PlayfabManager.playerCache.lastPlaytime = DateTime.Now.ToString(); //Update time last match BEGAN   
        GlobalManagers.Instance.PlayfabManager.playerCache.champion = ChampionSelection.options[ChampionSelection.value].text; //Record champion name

     }

    private void Start()
    {
        JoinRoomButton.onClick.AddListener(() => StartGame(GameMode.Client, EnterRoomCode.text));
        CreateRoomButton.onClick.AddListener(() => StartGame(GameMode.Host, EnterRoomCode.text));
        JoinRandomButton.onClick.AddListener(() => StartGame(GameMode.AutoHostOrClient, string.Empty));
    }

    private void ChangeInputFieldColor()
    {
        if (EnterName.text.Length < MIN_NAME_LENGTH)
        {
            EnterName.textComponent.color = INVALID_COLOR;
            EnterName.placeholder.color = INVALID_COLOR;
        }
        else
        {
            EnterName.textComponent.color = VALID_COLOR;
            EnterName.placeholder.color = VALID_COLOR;
        }

        if (EnterRoomCode.text.Length < MIN_ROOM_CODE_LENGTH)
        {
            EnterRoomCode.textComponent.color = INVALID_COLOR;
            EnterRoomCode.placeholder.color = INVALID_COLOR;
        }
        else
        {
            EnterRoomCode.textComponent.color = VALID_COLOR;
            EnterRoomCode.placeholder.color = VALID_COLOR;
        }
    }
    private void ActivateAndDeactivateButtons(bool changeColor = false)
    {
        if (EnterName.text.Length >= MIN_NAME_LENGTH && EnterRoomCode.text.Length >= MIN_ROOM_CODE_LENGTH)
        {
            JoinRoomButton.interactable = true;
            CreateRoomButton.interactable = true;

            if (changeColor)
            {
                JoinRoomButton.GetComponentInChildren<TextMeshProUGUI>().color = VALID_COLOR;
                CreateRoomButton.GetComponentInChildren<TextMeshProUGUI>().color = VALID_COLOR;
            }
        }
        else
        {
            JoinRoomButton.interactable = false;
            CreateRoomButton.interactable = false;

            if (changeColor)
            {
                JoinRoomButton.GetComponentInChildren<TextMeshProUGUI>().color = INVALID_COLOR;
                CreateRoomButton.GetComponentInChildren<TextMeshProUGUI>().color = INVALID_COLOR;
            }
        }
    }

    private void ActivateAndDeactivateJoinRandomButton(bool changeColor = false)
    {
        if (EnterName.text.Length >= MIN_NAME_LENGTH)
        {
            JoinRandomButton.interactable = true;

            if (changeColor)
            {
                JoinRandomButton.GetComponentInChildren<TextMeshProUGUI>().color = VALID_COLOR;
            }
        }
        else
        {
            JoinRandomButton.interactable = false;

            if (changeColor)
            {
                JoinRandomButton.GetComponentInChildren<TextMeshProUGUI>().color = INVALID_COLOR;
            }
        }
    }

    private void Update()
    {
        ChangeInputFieldColor();
        ActivateAndDeactivateButtons();
        ActivateAndDeactivateJoinRandomButton();
    }
}
