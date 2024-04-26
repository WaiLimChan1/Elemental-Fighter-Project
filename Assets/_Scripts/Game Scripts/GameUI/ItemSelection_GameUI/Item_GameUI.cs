using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item_GameUI : MonoBehaviour
{
    [Header("Item_GameUI Components")]
    [SerializeField] private int ItemIndex;
    [SerializeField] private Image ItemImage;
    [SerializeField] private Button SelectButton;

    static float CollideWithMouseRadius = 50.0f;

    public void SetItem(bool selectedItem, int index)
    {
        if (index < -1 || index >= ItemManager.Instance.Items.Length) return;

        if (selectedItem) SelectButton.gameObject.SetActive(false);
        else SelectButton.gameObject.SetActive(true);

        ItemIndex = index;
        ItemImage.sprite = ItemManager.Instance.Items[index].itemSprite;
    }

    public int GetItemIndex() { return ItemIndex; }

    public bool CollidingWithMouse()
    {
        return gameObject.activeSelf && Vector3.Distance(Input.mousePosition, transform.position) < CollideWithMouseRadius;
    }

    public void SetColor(Color color)
    {
        ItemImage.color = color;
    }

    public void LateUpdate()
    {
        if (CollidingWithMouse()) ItemImage.color = Color.grey;
        else ItemImage.color = Color.white;
    }

    public void OnClick()
    {
        if (!GameManager.CanUseGameManager()) return;
        if (GameManager.Instance.selectedItemLocal) return;

        GameManager.Instance.AddSelectedItem(ItemIndex);
    }
}
