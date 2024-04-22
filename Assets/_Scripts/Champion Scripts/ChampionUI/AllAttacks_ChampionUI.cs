using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Champion;

public class AllAttacks_ChampionUI : MonoBehaviour
{
    public static AllAttacks_ChampionUI Instance { get; private set; }

    [SerializeField] public Champion Champion;

    [Header("Champion Attack ChampionUI")]
    [SerializeField] public Attack_ChampionUI UniqueA;
    [SerializeField] public Attack_ChampionUI UniqueB;

    [SerializeField] public Attack_ChampionUI AirAttack;
    [SerializeField] public Attack_ChampionUI Attack1;
    [SerializeField] public Attack_ChampionUI Attack2;
    [SerializeField] public Attack_ChampionUI Attack3;
    [SerializeField] public Attack_ChampionUI SpecialAttak;
    [SerializeField] public Attack_ChampionUI Roll;

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

    public void Update()
    {
        if (Champion != null)
        {
            Champion.SetAttack_ChampionUI(this);
        }
    }

    public void SetAttack_ChampionUI(Attack_ChampionUI Attack_ChampionUI, Attack Attack, string KeyBind)
    {
        Attack_ChampionUI.gameObject.SetActive(true);
        Attack_ChampionUI.Attack = Attack;
        Attack_ChampionUI.AttackKeyBindText.text = KeyBind;
    }

    public void SetActiveAllAttack_ChampionUI(bool active)
    {
        UniqueA.gameObject.SetActive(active);
        UniqueB.gameObject.SetActive(active);
        AirAttack.gameObject.SetActive(active);
        Attack1.gameObject.SetActive(active);
        Attack2.gameObject.SetActive(active);
        Attack3.gameObject.SetActive(active);
        SpecialAttak.gameObject.SetActive(active);
        Roll.gameObject.SetActive(active);
    }
}
