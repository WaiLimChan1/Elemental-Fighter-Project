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
        public Sprite sprite;

        public float maxHealthBonus;
        public float maxManaBonus;

        public float healthRegenPercentageBonus;
        public float manaRegenPercentageBonus;

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

        public void CombineWithItem(Item item)
        {
            maxHealthBonus += item.maxHealthBonus;
            maxManaBonus += item.maxManaBonus;

            healthRegenPercentageBonus += item.healthRegenPercentageBonus;
            manaRegenPercentageBonus += item.manaRegenPercentageBonus;

            ultimateMeterRegenBonus += item.ultimateMeterRegenBonus;
            ultimateMeterGainMultipierBonus += item.ultimateMeterGainMultipierBonus;

            armorBonus += item.armorBonus;
            crowdControlIgnorePercentageBonus += item.crowdControlIgnorePercentageBonus;

            blockPercentageBonus += item.blockPercentageBonus;
            crowdControlBlockPercentageBonus += item.crowdControlBlockPercentageBonus;

            physicalDamageBonus += item.physicalDamageBonus;
            attackSpeedBonus += item.attackSpeedBonus;
            coolDownReductionBonus += item.coolDownReductionBonus;
            omnivampBonus += item.omnivampBonus;

            mobilityModifierBonus += item.mobilityModifierBonus;
        }
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
