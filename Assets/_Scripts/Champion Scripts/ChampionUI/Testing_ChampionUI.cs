using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Testing_ChampionUI : MonoBehaviour
{
    public static Testing_ChampionUI Instance { get; private set; }
    public NetworkedPlayer NetworkedPlayer;

    [SerializeField] private GameObject Content;

    [Header("Testing_UI Resource Components")]
    [SerializeField] private Button RestoreHealthAndManaButton;
    [SerializeField] private Button ClearHealthAndManaButton;

    [SerializeField] private Button FillUltimateMeterButton;
    [SerializeField] private Button ClearUltimateMeterButton;

    [Header("Testing_UI Item Components")]
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
        ClearHealthAndManaButton.onClick.AddListener(() => ClearHealthAndMana());
        FillUltimateMeterButton.onClick.AddListener(() => FillUltimateMeter());
        ClearUltimateMeterButton.onClick.AddListener(() => ClearUltimateMeter());
        AddItemButton.onClick.AddListener(() => AddItem());
        RemoveAllItemsButton.onClick.AddListener(() => RemoveAllItems());
    }

    private bool NetworkedPlayerHasChampion()
    {
        if (NetworkedPlayer == null || NetworkedPlayer.Object == default) return false;
        if (NetworkedPlayer.OwnedChampion == null || NetworkedPlayer.OwnedChampion == default) return false;
        if (NetworkedPlayer.OwnedChampion.GetComponent<Champion>() == null) return false;
        return true;
    }

    private void RestoreHealthAndMana()
    {
        if (!NetworkedPlayerHasChampion()) return;
        NetworkedPlayer.OwnedChampion.GetComponent<Champion>().RPC_RestoreHealthAndMana();
    }

    private void ClearHealthAndMana()
    {
        if (!NetworkedPlayerHasChampion()) return;
        NetworkedPlayer.OwnedChampion.GetComponent<Champion>().RPC_ClearHealthAndMana();
    }

    private void FillUltimateMeter()
    {
        if (!NetworkedPlayerHasChampion()) return;
        NetworkedPlayer.OwnedChampion.GetComponent<Champion>().RPC_FillUltimateMeter();
    }

    private void ClearUltimateMeter()
    {
        if (!NetworkedPlayerHasChampion()) return;
        NetworkedPlayer.OwnedChampion.GetComponent<Champion>().RPC_ClearUltimateMeter();
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
