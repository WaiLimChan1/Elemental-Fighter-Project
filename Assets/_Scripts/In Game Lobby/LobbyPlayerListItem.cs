using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyPlayerListItem : MonoBehaviour
{
    [Header("LobbyPlayerListItem Components")]
    [SerializeField] private TextMeshProUGUI PlayerNameText;

    public void SetName(string name)
    {
        PlayerNameText.text = name;
    }
}
