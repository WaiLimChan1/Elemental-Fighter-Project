using Fusion;
using System;
using UnityEngine;
using static Champion;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    public Sprite EmptyItemSprite;

    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Item Class
    [Serializable]
    public class Item
    {
        public string itemName;
        public Sprite itemSprite;

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

        public string GetAllStatsAsAStringList()
        {
            string StatsList = "";

            StatsList += "Max Health: " + maxHealthBonus + "\n";
            StatsList += "Max Mana: " + maxManaBonus + "\n";

            StatsList += "Health Regen: " + healthRegenPercentageBonus + "\n";
            StatsList += "Mana Regen: " + manaRegenPercentageBonus + "\n";

            StatsList += "Ultimate Meter Regen: " + ultimateMeterRegenBonus + "\n";
            StatsList += "Ultimate Meter Gain Multiplier: " + ultimateMeterGainMultipierBonus + "\n";

            StatsList += "Armor: " + armorBonus + "\n";
            StatsList += "CC Ignore: " + crowdControlIgnorePercentageBonus + "\n";

            StatsList += "Block: " + blockPercentageBonus + "\n";
            StatsList += "CC Block: " + crowdControlBlockPercentageBonus + "\n";

            StatsList += "Physics Damage: " + physicalDamageBonus + "\n";
            StatsList += "Attack Speed: " + attackSpeedBonus + "\n";
            StatsList += "CDR: " + coolDownReductionBonus + "\n";
            StatsList += "Omnivamp: " + omnivampBonus + "\n";

            StatsList += "Mobility Modifier: " + mobilityModifierBonus + "\n";

            return StatsList;
        }

        public string GetAllNonZeroStatsAsAStringList()
        {
            string StatsList = "";

            if (maxHealthBonus > 0) StatsList += "Max Health: " + maxHealthBonus + "\n";
            if (maxManaBonus > 0) StatsList += "Max Mana: " + maxManaBonus + "\n";

            if (healthRegenPercentageBonus > 0) StatsList += "Health Regen: " + healthRegenPercentageBonus + "\n";
            if (manaRegenPercentageBonus > 0) StatsList += "Mana Regen: " + manaRegenPercentageBonus + "\n";

            if (ultimateMeterRegenBonus > 0) StatsList += "Ultimate Meter Regen: " + ultimateMeterRegenBonus + "\n";
            if (ultimateMeterGainMultipierBonus > 0) StatsList += "Ultimate Meter Gain Multiplier: " + ultimateMeterGainMultipierBonus + "\n";

            if (armorBonus > 0) StatsList += "Armor: " + armorBonus + "\n";
            if (crowdControlIgnorePercentageBonus > 0) StatsList += "CC Ignore: " + crowdControlIgnorePercentageBonus + "\n";

            if (blockPercentageBonus > 0) StatsList += "Block: " + blockPercentageBonus + "\n";
            if (crowdControlBlockPercentageBonus > 0) StatsList += "CC Block: " + crowdControlBlockPercentageBonus + "\n";

            if (physicalDamageBonus > 0) StatsList += "Physics Damage: " + physicalDamageBonus + "\n";
            if (attackSpeedBonus > 0) StatsList += "Attack Speed: " + attackSpeedBonus + "\n";
            if (coolDownReductionBonus > 0) StatsList += "CDR: " + coolDownReductionBonus + "\n";
            if (omnivampBonus > 0) StatsList += "Omnivamp: " + omnivampBonus + "\n";

            if (mobilityModifierBonus > 0) StatsList += "Mobility Modifier: " + mobilityModifierBonus + "\n";

            return StatsList;
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
