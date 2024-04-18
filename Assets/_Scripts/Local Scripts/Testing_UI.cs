using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Testing_UI : MonoBehaviour
{
    public static Testing_UI Instance { get; private set; }
    public NetworkedPlayer NetworkedPlayer;

    [Header("Testing_UI Components")]
    [SerializeField] private Button RestoreHealthAndManaButton;
    [SerializeField] private Button FillUltimateMeterButton;

    [SerializeField] private TMP_Dropdown ItemSelection;
    [SerializeField] private Button AddItemButton;
    [SerializeField] private Button RemoveAllItemsButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void InitializeItemSelection()
    {
        // Clear any existing options
        ItemSelection.ClearOptions();

        // Create a list of dropdown options
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < ItemManager.Instance.Items.Length; i++)
            options.Add(new TMP_Dropdown.OptionData(ItemManager.Instance.Items[i].itemName, ItemManager.Instance.Items[i].itemSprite, Color.white));

        // Add options to the dropdown
        ItemSelection.AddOptions(options);
    }

    private void Start()
    {
        InitializeItemSelection();

        RestoreHealthAndManaButton.onClick.AddListener(() => RestoreHealthAndMana());
        FillUltimateMeterButton.onClick.AddListener(() => FillUltimateMeter());
        AddItemButton.onClick.AddListener(() => AddItem());
        RemoveAllItemsButton.onClick.AddListener(() => RemoveAllItems());
    }

    private void RestoreHealthAndMana()
    {
        if (NetworkedPlayer == null) return;
        if (NetworkedPlayer.OwnedChampion == null) return;
        if (NetworkedPlayer.OwnedChampion.GetComponent<Champion>() == null) return;

        NetworkedPlayer.OwnedChampion.GetComponent<Champion>().RPC_RestoreHealthAndMana();
    }

    private void FillUltimateMeter()
    {
        if (NetworkedPlayer == null) return;
        if (NetworkedPlayer.OwnedChampion == null) return;
        if (NetworkedPlayer.OwnedChampion.GetComponent<Champion>() == null) return;

        NetworkedPlayer.OwnedChampion.GetComponent<Champion>().RPC_FillUltimateMeter();
    }

    private void AddItem()
    {
        if (NetworkedPlayer == null) return;

        NetworkedPlayer.RPC_AddItem(ItemSelection.value);
    }

    private void RemoveAllItems()
    {
        if (NetworkedPlayer == null) return;

        NetworkedPlayer.RPC_RemoveAllItems();
    }
}
