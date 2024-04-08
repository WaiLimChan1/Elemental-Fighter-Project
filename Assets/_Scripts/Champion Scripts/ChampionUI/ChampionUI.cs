using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Champion;

public class ChampionUI : MonoBehaviour
{
    [SerializeField] public Champion Champion;

    [Header("Champion Attack ChampionUI")]
    [SerializeField] public Attack_ChampionUI Unique1;
    [SerializeField] public Attack_ChampionUI Unique2;

    [SerializeField] public Attack_ChampionUI AirAttack;
    [SerializeField] public Attack_ChampionUI Attack1;
    [SerializeField] public Attack_ChampionUI Attack2;
    [SerializeField] public Attack_ChampionUI Attack3;
    [SerializeField] public Attack_ChampionUI SpecialAttak;

    public void Update()
    {
        if (Champion != null)
        {
            AirAttack.Attack = Champion.Attacks[0];
            Attack1.Attack = Champion.Attacks[1];
            Attack2.Attack = Champion.Attacks[2];
            Attack3.Attack = Champion.Attacks[3];
            SpecialAttak.Attack = Champion.Attacks[4];
        }
    }
}
