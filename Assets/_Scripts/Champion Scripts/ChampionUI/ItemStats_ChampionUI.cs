using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemStats_ChampionUI : MonoBehaviour
{
    [Header("ItemStats_ChampionUI Components")]
    [SerializeField] private Image Background;
    [SerializeField] private TextMeshProUGUI ItemName;
    [SerializeField] private TextMeshProUGUI StatsList;

    public void SetUp(int itemIndex)
    {
        if (itemIndex < -1 || itemIndex >= ItemManager.Instance.Items.Length) return;
        ItemManager.Item item = ItemManager.Instance.Items[itemIndex];

        ItemName.text = item.itemName;
        StatsList.text = item.GetAllNonZeroStatsAsAStringList();
    }
}
