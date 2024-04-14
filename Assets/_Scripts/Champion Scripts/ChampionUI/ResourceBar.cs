using Fusion;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ResourceBar : MonoBehaviour
{
    [Header("Resource Bar Components")]
    [SerializeField] private GameObject ResourceBarContent;
    [SerializeField] private TextMeshProUGUI PlayerNameText;
    [SerializeField] private Image HealthBar;
    [SerializeField] private TextMeshProUGUI HealthAmountText;
    [SerializeField] private Image ManaBar;
    [SerializeField] private TextMeshProUGUI ManaAmountText;
    [SerializeField] private GameObject UltimateMeterFrame;
    [SerializeField] private Image UltimateMeterBar;
    [SerializeField] private TextMeshProUGUI UltimateMeterAmountText;

    public void SetPlayerNameText(string name)
    {
        PlayerNameText.text = name;
    }

    public void Awake()
    {
        ResourceBarContent.SetActive(false);
    }

    public void UpdateResourceBarVisuals(float healthNetworked, float maxHealth, float manaNetworked, float maxMana, float ultimateMeterNetworked, float ultimateMeterCost)
    {
        if (healthNetworked <= 0) ResourceBarContent.SetActive(false);
        else ResourceBarContent.SetActive(true);

        HealthBar.fillAmount = healthNetworked / maxHealth;
        HealthAmountText.text = $"{(int)healthNetworked}/{(int)maxHealth}";

        ManaBar.fillAmount = manaNetworked / maxMana;
        ManaAmountText.text = $"{(int)manaNetworked}/{(int)maxMana}";

        if (ultimateMeterCost <= 0)
        {
            UltimateMeterFrame.SetActive(false);
        }
        else
        {
            UltimateMeterFrame.SetActive(true);
            UltimateMeterBar.fillAmount = Mathf.Clamp01(ultimateMeterNetworked / ultimateMeterCost);

            if (ultimateMeterNetworked >= ultimateMeterCost)
            {
                UltimateMeterAmountText.text = "Transform (E)";
            }
            else
            {
                int round = 2;
                UltimateMeterAmountText.text = (int)(ultimateMeterNetworked / ultimateMeterCost * 100 * Mathf.Pow(10, round)) / Mathf.Pow(10, round) + "%";
            }
        }
    }
}