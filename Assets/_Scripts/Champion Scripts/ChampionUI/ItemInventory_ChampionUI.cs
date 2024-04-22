using UnityEngine;

public class ItemInventory_ChampionUI : MonoBehaviour
{
    public static ItemInventory_ChampionUI Instance { get; private set; }

    [Header("Champion Attack ChampionUI")]
    [SerializeField] public Item_ChampionUI[] Item_ChampionUIs;

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

    public void SetIndividualItemImage(int Item_ChampionUIs_Index, int itemIndex)
    {
        if (Item_ChampionUIs_Index < 0 || Item_ChampionUIs_Index >= Item_ChampionUIs.Length) return;
        if (itemIndex < -1 || itemIndex >= ItemManager.Instance.Items.Length) return;

        if (itemIndex == -1)
        {

            Item_ChampionUIs[Item_ChampionUIs_Index].SetItemImage(ItemManager.Instance.EmptyItemSprite);
            Item_ChampionUIs[Item_ChampionUIs_Index].gameObject.SetActive(false);
        }
        else
        {
            Item_ChampionUIs[Item_ChampionUIs_Index].SetItemImage(ItemManager.Instance.Items[itemIndex].itemSprite);
            Item_ChampionUIs[Item_ChampionUIs_Index].gameObject.SetActive(true);
        }
    }
}

