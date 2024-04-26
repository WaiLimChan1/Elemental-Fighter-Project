using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class ItemSelection_GameUI : MonoBehaviour
{
    [Header("ItemSelection_GameUI Components")]
    [SerializeField] private TextMeshProUGUI Title;

    [Header("Items")]
    [SerializeField] public Item_GameUI[] Item_GameUIs;

    [Header("Item Stats")]
    [SerializeField] private TextMeshProUGUI SelectedItemName;
    [SerializeField] private TextMeshProUGUI SelectedItemStatsList;

    public void UpdateItem_GameUIs(bool selectedItem, List<int> itemIndexList)
    {
        foreach (Item_GameUI Item_GameUI in Item_GameUIs) Item_GameUI.gameObject.SetActive(false);

        for (int i = 0; i < Item_GameUIs.Length && i < itemIndexList.Count; i++)
        {
            if (itemIndexList[i] < -1 || itemIndexList[i] >= ItemManager.Instance.Items.Length) continue;

            Item_GameUIs[i].gameObject.SetActive(true);
            Item_GameUIs[i].SetItem(selectedItem, itemIndexList[i]);
        }
    }

    public void UpdateItemStats()
    {
        SelectedItemName.gameObject.SetActive(false);
        SelectedItemStatsList.gameObject.SetActive(false);

        for (int i = 0; i < Item_GameUIs.Length; i++)
        {
            if (Item_GameUIs[i].CollidingWithMouse())
            {
                int itemIndex = Item_GameUIs[i].GetItemIndex();
                if (itemIndex < -1 || itemIndex >= ItemManager.Instance.Items.Length) continue;

                ItemManager.Item item = ItemManager.Instance.Items[itemIndex];

                SelectedItemName.gameObject.SetActive(true);
                SelectedItemStatsList.gameObject.SetActive(true);

                SelectedItemName.text = item.itemName;
                SelectedItemStatsList.text = item.GetAllNonZeroStatsAsAStringList();
                break;
            }
        }
    }

    public void UpdateGameUI(bool selectedItem, float remainingTime, List<int> itemIndexList)
    {
        if (selectedItem) Title.text = $"You've Selected An Item ({remainingTime})";
        else Title.text = $"Select An Item ({remainingTime})";

        UpdateItem_GameUIs(selectedItem, itemIndexList);
        UpdateItemStats();
    }
}
