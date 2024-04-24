using UnityEngine;
using UnityEngine.UI;

public class Item_ChampionUI : MonoBehaviour
{
    [Header("Item_ChampionUI Components")]
    [SerializeField] private GameObject Content;
    [SerializeField] private Image ItemImage;
    [SerializeField] private int ItemIndex;

    static float CollideWithMouseRadius = 50.0f;

    public void SetItem(int index)
    {
        ItemIndex = index;
    }

    public int GetItemIndex() { return ItemIndex; }   

    public void SetItemImage(Sprite sprite)
    {
        ItemImage.sprite = sprite;
    }

    public bool CollidingWithMouse()
    {
        return gameObject.activeSelf && Vector3.Distance(Input.mousePosition, transform.position) < CollideWithMouseRadius;
    }

    public void Update()
    {
        if (Vector3.Distance(Input.mousePosition, transform.position) < CollideWithMouseRadius)
        {
            ItemImage.color = Color.grey;
        }
        else
        {
            ItemImage.color = Color.white;
        }    
    }
}
