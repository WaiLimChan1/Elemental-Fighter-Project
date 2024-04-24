using UnityEngine;

public class ItemInventory_ChampionUI : MonoBehaviour
{
    public static ItemInventory_ChampionUI Instance { get; private set; }

    [Header("Champion Attack ChampionUI")]
    [SerializeField] public Item_ChampionUI[] Item_ChampionUIs;
    [SerializeField] public ItemStats_ChampionUI ItemStats_ChampionUI;

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

    public void SetIndividualItem_ChampionUI(int Item_ChampionUIs_Index, int itemIndex)
    {
        if (Item_ChampionUIs_Index < 0 || Item_ChampionUIs_Index >= Item_ChampionUIs.Length) return;
        if (itemIndex < -1 || itemIndex >= ItemManager.Instance.Items.Length) return;

        if (itemIndex == -1)
        {
            Item_ChampionUIs[Item_ChampionUIs_Index].SetItem(-1);
            Item_ChampionUIs[Item_ChampionUIs_Index].SetItemImage(ItemManager.Instance.EmptyItemSprite);
            Item_ChampionUIs[Item_ChampionUIs_Index].gameObject.SetActive(false);
        }
        else
        {
            Item_ChampionUIs[Item_ChampionUIs_Index].SetItem(itemIndex);
            Item_ChampionUIs[Item_ChampionUIs_Index].SetItemImage(ItemManager.Instance.Items[itemIndex].itemSprite);
            Item_ChampionUIs[Item_ChampionUIs_Index].gameObject.SetActive(true);
        }
    }

    public void Update()
    {
        ItemStats_ChampionUI.gameObject.SetActive(false);

        for (int i = 0; i < Item_ChampionUIs.Length; i++)
        {
            if (Item_ChampionUIs[i].CollidingWithMouse())
            {
                ItemStats_ChampionUI.gameObject.SetActive(true);
                ItemStats_ChampionUI.SetUp(Item_ChampionUIs[i].GetItemIndex());
                break;
            }
        }
    }
}

