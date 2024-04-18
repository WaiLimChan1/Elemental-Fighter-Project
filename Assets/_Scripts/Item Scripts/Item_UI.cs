using UnityEngine;

public class Item_UI : MonoBehaviour
{
    public static Item_UI Instance { get; private set; }

    [Header("Champion Attack ChampionUI")]
    [SerializeField] public Individual_Item_UI[] Individual_Item_UI;

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

    public void SetIndividualItemImage(int Individual_Item_UI_Index, int itemIndex)
    {
        if (Individual_Item_UI_Index < 0 || Individual_Item_UI_Index >= Individual_Item_UI.Length) return;
        if (itemIndex < -1 || itemIndex >= ItemManager.Instance.Items.Length) return;

        if (itemIndex == -1)
        {

            Individual_Item_UI[Individual_Item_UI_Index].SetItemImage(ItemManager.Instance.EmptyItemSprite);
            Individual_Item_UI[Individual_Item_UI_Index].gameObject.SetActive(false);
        }
        else
        {
            Individual_Item_UI[Individual_Item_UI_Index].SetItemImage(ItemManager.Instance.Items[itemIndex].itemSprite);
            Individual_Item_UI[Individual_Item_UI_Index].gameObject.SetActive(true);
        }
    }
}

