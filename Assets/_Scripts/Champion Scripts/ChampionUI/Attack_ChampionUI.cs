using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Attack_ChampionUI : MonoBehaviour
{
    [SerializeField] public Champion.Attack Attack;

    [SerializeField] private GameObject Content;
    [SerializeField] private Image AttackImage;
    [SerializeField] private Image CoolDownCoverImage;
    [SerializeField] private TextMeshProUGUI AttackManaCostText;
    [SerializeField] private TextMeshProUGUI AttackCoolDownDurationText;
    [SerializeField] private TextMeshProUGUI AttackCoolDownTimerText;

    public void Update()
    {
        AttackManaCostText.text = "" + Attack.manaCost;
        AttackCoolDownDurationText.text = "" + Attack.coolDownDuration;

        float coolDownRemainingTime = Attack.getCoolDownRemainingTime();
        if (coolDownRemainingTime == 0)
        {
            AttackCoolDownTimerText.gameObject.SetActive(false);
            CoolDownCoverImage.fillAmount = 0;
        }
        else
        {
            AttackCoolDownTimerText.gameObject.SetActive(true);
            AttackCoolDownTimerText.text = "" + Mathf.Round(coolDownRemainingTime * 10) / 10f + "s";
            CoolDownCoverImage.fillAmount = coolDownRemainingTime / Attack.coolDownDuration;
        }
    }
}
