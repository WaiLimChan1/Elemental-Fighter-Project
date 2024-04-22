using Fusion;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.Unicode;

public class ResourceBar : MonoBehaviour
{
    [Header("Resource Bar Components")]
    [SerializeField] private GameObject ResourceBarContent;

    [SerializeField] private TextMeshProUGUI PlayerNameText;

    [SerializeField] private Image HealthBar;
    [SerializeField] private TextMeshProUGUI HealthAmountText;

    [SerializeField] private Image ManaBar;
    [SerializeField] private TextMeshProUGUI ManaAmountText;

    [SerializeField] private Image TakeHitRecoveryPercentageBar;

    [SerializeField] private GameObject UltimateMeterFrame;
    [SerializeField] private Image UltimateMeterBar;
    [SerializeField] private TextMeshProUGUI UltimateMeterAmountText;

    [SerializeField] private GameObject CanTransform;

    public void SetPlayerNameText(string name)
    {
        PlayerNameText.text = name;
    }

    public void Awake()
    {
        ResourceBarContent.SetActive(false);
    }

    public void UpdateResourceBarVisuals(float healthNetworked, float maxHealth, float manaNetworked, float maxMana, float takeHitRecoveryPercentage, float ultimateMeterNetworked, float ultimateMeterCost, bool defaultForm = true)
    {
        if (healthNetworked <= 0) ResourceBarContent.SetActive(false);
        else ResourceBarContent.SetActive(true);

        HealthBar.fillAmount = healthNetworked / maxHealth;
        HealthAmountText.text = $"{(int)healthNetworked}/{(int)maxHealth}";

        ManaBar.fillAmount = manaNetworked / maxMana;
        ManaAmountText.text = $"{(int)manaNetworked}/{(int)maxMana}";

        if (takeHitRecoveryPercentage <= 0) TakeHitRecoveryPercentageBar.gameObject.SetActive(false);
        else
        {
            TakeHitRecoveryPercentageBar.gameObject.SetActive(true);
            TakeHitRecoveryPercentageBar.fillAmount = takeHitRecoveryPercentage;
        }

        if (ultimateMeterCost <= 0)
        {
            UltimateMeterFrame.SetActive(false);
            CanTransform.SetActive(false);
        }
        else
        {
            UltimateMeterFrame.SetActive(true);
            UltimateMeterBar.fillAmount = Mathf.Clamp01(ultimateMeterNetworked / ultimateMeterCost);

            int round;
            if (defaultForm) round = 0;
            else round = 0;

            string percent = (int)(ultimateMeterNetworked / ultimateMeterCost * 100 * Mathf.Pow(10, round)) / Mathf.Pow(10, round) + "%";

            if (ultimateMeterNetworked >= ultimateMeterCost && defaultForm) CanTransform.SetActive(true);
            else CanTransform.SetActive(false);

            UltimateMeterAmountText.text = percent;
        }
    }
}