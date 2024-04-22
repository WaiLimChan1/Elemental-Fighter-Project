using UnityEngine;
using UnityEngine.UI;

public class Item_ChampionUI : MonoBehaviour
{
    [Header("Item_ChampionUI Components")]
    [SerializeField] private GameObject Content;
    [SerializeField] private Image ItemImage;

    public void SetItemImage(Sprite sprite)
    {
        ItemImage.sprite = sprite;
    }
}
