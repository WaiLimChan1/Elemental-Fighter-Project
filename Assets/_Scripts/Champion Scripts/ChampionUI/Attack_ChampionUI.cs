using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Attack_ChampionUI : MonoBehaviour
{
    [Header("Attack_ChampionUI Scripts")]
    [SerializeField] public Champion.Attack Attack;

    [Header("Attack_ChampionUI Components")]
    [SerializeField] private GameObject Content;
    [SerializeField] private Image AttackImage;
    [SerializeField] private Image CoolDownCoverImage;
    [SerializeField] private Image InsufficientManaCoverImage;
    [SerializeField] private TextMeshProUGUI AttackNameText;
    [SerializeField] public TextMeshProUGUI AttackKeyBindText;
    [SerializeField] private TextMeshProUGUI AttackManaCostText;
    [SerializeField] private TextMeshProUGUI AttackCoolDownDurationText;
    [SerializeField] private TextMeshProUGUI AttackCoolDownTimerText;

    public void LateUpdate()
    {
        if (AllAttacks_ChampionUI.Instance.Champion == null || AllAttacks_ChampionUI.Instance.Champion.Object == default) return;

        AttackNameText.text = Attack.attackName;
        AttackManaCostText.text = "" + Attack.manaCost;
        AttackCoolDownDurationText.text = "" + Mathf.Round(AllAttacks_ChampionUI.Instance.Champion.getCoolDownDuration(Attack) * 10) / 10f + "s";

        float coolDownRemainingTime = Attack.getCoolDownRemainingTime();
        if (coolDownRemainingTime == 0)
        {
            AttackNameText.gameObject.SetActive(true);
            AttackCoolDownTimerText.gameObject.SetActive(false);
            CoolDownCoverImage.fillAmount = 0;
        }
        else
        {
            AttackNameText.gameObject.SetActive(false);
            AttackCoolDownTimerText.gameObject.SetActive(true);
            AttackCoolDownTimerText.text = "" + Mathf.Round(coolDownRemainingTime * 10) / 10f + "s";
            CoolDownCoverImage.fillAmount = coolDownRemainingTime / AllAttacks_ChampionUI.Instance.Champion.getCoolDownDuration(Attack);
        }

        if (AllAttacks_ChampionUI.Instance.Champion != null && AllAttacks_ChampionUI.Instance.Champion.Object != default)
        {
            if (AllAttacks_ChampionUI.Instance.Champion.manaNetworked < Attack.manaCost) InsufficientManaCoverImage.gameObject.SetActive(true);
            else InsufficientManaCoverImage.gameObject.SetActive(false);
        }
    }
}
