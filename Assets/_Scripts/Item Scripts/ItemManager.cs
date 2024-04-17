using Fusion;
using System;
using UnityEngine;
using static Champion;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Item Class
    [Serializable]
    public class Item
    {
        public string itemName;

        public float maxHealthBonus;
        public float maxManaBonus;

        public float healthRegenBonus;
        public float manaRegenBonus;

        public float ultimateMeterRegenBonus;
        public float ultimateMeterGainMultipierBonus;

        public float armorBonus;
        public float crowdControlIgnorePercentageBonus;

        public float blockPercentageBonus;
        public float crowdControlBlockPercentageBonus;

        public float physicalDamageBonus;
        public float attackSpeedBonus;
        public float coolDownReductionBonus;
        public float omnivampBonus;

        public float mobilityModifierBonus;
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    [Header("Items")]
    [SerializeField] public Item[] Items;
    //---------------------------------------------------------------------------------------------------------------------------------------------

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
    }
}
