using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChampionStats_ChampionUI : MonoBehaviour
{
    public static ChampionStats_ChampionUI Instance { get; private set; }

    [SerializeField] public Champion Champion;

    [Header("Champion Stats ChampionUI")]
    [SerializeField] private GameObject Content;
    [SerializeField] private TextMeshProUGUI StatsList;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        Content.SetActive(false);
    }

    public void Update()
    {
        if (Champion != null)
        {
            if (Input.GetKey(KeyCode.C))
            {
                Content.SetActive(true);
                ItemManager.Item item = Champion.GetCalculatedStatsAsAnItem();
                StatsList.text = item.GetAllStatsAsAStringList();
            }
            else Content.SetActive(false);
        }
    }
}
