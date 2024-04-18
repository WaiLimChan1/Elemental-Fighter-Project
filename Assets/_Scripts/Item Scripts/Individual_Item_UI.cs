using UnityEngine;
using UnityEngine.UI;

public class Individual_Item_UI : MonoBehaviour
{
    [Header("Individual_Item_UI Components")]
    [SerializeField] private GameObject Content;
    [SerializeField] private Image ItemImage;

    public void SetItemImage(Sprite sprite)
    {
        ItemImage.sprite = sprite;
    }
}
