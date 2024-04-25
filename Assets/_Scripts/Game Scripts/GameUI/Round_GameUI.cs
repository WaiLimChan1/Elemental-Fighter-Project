using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Round_GameUI : MonoBehaviour
{
    [Header("Round_GameUI Components")]
    [SerializeField] private TextMeshProUGUI RemainingTimeText;

    public void UpdateGameUI(float remainingTime)
    {
        RemainingTimeText.text = "Remaining Time: " + remainingTime + "s";
    }
}
