using Fusion;
using TMPro;
using Unity.Burst;
using UnityEngine;
using UnityEngine.UI;

public class ResourceBar : MonoBehaviour
{
    [Header("Resource Bar Components")]
    [SerializeField] private TextMeshProUGUI PlayerNameText;
    [SerializeField] private Image HealthBar;
    [SerializeField] private TextMeshProUGUI HealthAmountText;
    [SerializeField] private Image ManaBar;
    [SerializeField] private TextMeshProUGUI ManaAmountText;
    [SerializeField] private Image UltimateMeterBar;
    [SerializeField] private TextMeshProUGUI UltimateMeterAmountText;

    public void SetPlayerNameText(string name)
    {
        PlayerNameText.text = name;
    }

    public void UpdateResourceBarVisuals(float healthNetworked, float maxHealth, float manaNetworked, float maxMana)
    {
        HealthBar.fillAmount = healthNetworked / maxHealth;
        HealthAmountText.text = $"{(int)healthNetworked}/{(int)maxHealth}";

        ManaBar.fillAmount = manaNetworked / maxMana;
        ManaAmountText.text = $"{(int)manaNetworked}/{(int)maxMana}";
    }
}

//Hi 
// Hello!