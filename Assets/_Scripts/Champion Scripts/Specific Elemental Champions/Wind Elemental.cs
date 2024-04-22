using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindElemental : ElementalChampion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Wind Elemental Variables")]
    [SerializeField][Networked] private Champion blinkTarget { get; set; }
    [SerializeField] private float blinkRange = 20f;
    [SerializeField] private Attack blink;

    [SerializeField][Networked] private Champion specialAttackTarget { get; set; }
    [SerializeField] private float SpecialAttackTeleportRange = 25f;
    [SerializeField] private BoxCollider2D SpecialAttackPartII;
    [SerializeField] private float SpecialAttackPartIIDamageMultiplier = 5f;
    private const float SpecialPartIICutOffTime = 15.0f / 22.0f;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Attack Variables & Attack Functions
    //Status.UNIQUE2 : Blink

    public override void SetAttack_ChampionUI(AllAttacks_ChampionUI ChampionUI)
    {
        base.SetAttack_ChampionUI(ChampionUI);
        ChampionUI.SetAttack_ChampionUI(ChampionUI.UniqueB, blink, "Q");
    }

    protected override Attack getAttack(Status status)
    {
        if (status == Status.UNIQUE2) return blink;
        return base.getAttack(status);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    //Status.UNIQUE2 : Blink

    protected override bool SingleAnimationStatus()
    {
        return (base.SingleAnimationStatus() ||
            status == Status.UNIQUE2);
    }

    protected override bool UnstoppableStatusNetworked()
    {
        return base.UnstoppableStatusNetworked() && statusNetworked != Status.SPECIAL_ATTACK;
    }

    protected override bool AttackSpeedStatus(Status status)
    {
        return (status == Status.AIR_ATTACK || status == Status.ATTACK1 || status == Status.ATTACK2 || status == Status.ATTACK3 || status == Status.SPECIAL_ATTACK);
    }

    protected override void OnGroundTakeInput()
    {
        base.OnGroundTakeInput();
        if (Input.GetKeyDown(KeyCode.Q) && CanUseAttack(Status.UNIQUE2)) status = Status.UNIQUE2; //Blink
    }

    protected override void CancelTakeInput()
    {
        base.CancelTakeInput();
        if (status == Status.BEGIN_DEFEND || status == Status.DEFEND) status = Status.IDLE; //Wind Elemental does not have Defend
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void DealDamageToVictim(Champion enemy, float damage, float numOfAttacks)
    {
        if (statusNetworked == Status.AIR_ATTACK || statusNetworked == Status.UNIQUE1)
            enemy.TakeDamageNetworked(this, damage, numOfAttacks, isFacingLeftNetworked, AttackType.BlockByFacingAttacker, transform.position);
        else enemy.TakeDamageNetworked(this, damage, numOfAttacks, isFacingLeftNetworked, AttackType.BlockByFacingAttack);
    }

    public override void GetAttackBoxAndDamage(ref BoxCollider2D attackBox, ref float damage, ref float numOfAttacks, int index)
    {
        if (statusNetworked == Status.SPECIAL_ATTACK && ChampionAnimationController.GetNormalizedTime() >= SpecialPartIICutOffTime)
        {
            attackBox = SpecialAttackPartII;
            damage = getCalculatedDamage(Attacks[index]) * SpecialAttackPartIIDamageMultiplier;
            numOfAttacks = 1;
        }
        else base.GetAttackBoxAndDamage(ref attackBox, ref damage, ref numOfAttacks, index);
    }

    public override void GetControlBoxAndStrength(ref BoxCollider2D crowdControlBox, ref float crowdControlStrength, int index)
    {
        if (statusNetworked == Status.SPECIAL_ATTACK && ChampionAnimationController.GetNormalizedTime() >= SpecialPartIICutOffTime)
        {
            crowdControlBox = SpecialAttackPartII;
            crowdControlStrength = Attacks[index].crowdControlStrength;
        }
        else
        {
            crowdControlBox = Attacks[index].hitBox;
            crowdControlStrength = Attacks[index].crowdControlStrength;
        }
    }

    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.AIR_ATTACK) enemy.SetVelocity(new Vector2(0, crowdControlStrength));
        else if (statusNetworked == Status.ATTACK1) enemy.SetVelocity(new Vector2(direction * crowdControlStrength, crowdControlStrength));
        else if (statusNetworked == Status.ATTACK2) enemy.SetVelocity(new Vector2(direction * crowdControlStrength, 0));
        else if (statusNetworked == Status.ATTACK3) enemy.SetVelocity(new Vector2(direction * crowdControlStrength, crowdControlStrength));
        else if (statusNetworked == Status.SPECIAL_ATTACK) enemy.AddVelocity(new Vector2(direction * crowdControlStrength, 0));
        else if (statusNetworked == Status.UNIQUE1)
        {
            if (enemy.transform.position.x < transform.position.x) enemy.AddVelocity(new Vector2(-1 * crowdControlStrength, crowdControlStrength));
            else enemy.AddVelocity(new Vector2(crowdControlStrength, crowdControlStrength));
        }
    }

    public override void AnimationTriggerMobility()
    {
        if (statusNetworked == Status.SPECIAL_ATTACK)
        {
            if (specialAttackTarget == null) specialAttackTarget = MainGameUtils.FindClosestEnemyCircle(this, transform.position, SpecialAttackTeleportRange);
            if (specialAttackTarget != null)
            {
                Vector3 linkPoint = new Vector3(Attacks[4].hitBox.offset.x, Collider.offset.y);
                Vector3 changeVector = specialAttackTarget.transform.TransformPoint(specialAttackTarget.Collider.offset) - AttackBoxesParent.TransformPoint(linkPoint);
                transform.position = transform.position + changeVector;
            }
        }
        else if (statusNetworked == Status.UNIQUE2)
        {
            if (blinkTarget == null) blinkTarget = MainGameUtils.FindClosestEnemyCircle(this, transform.position, blinkRange);
            if (blinkTarget != null)
            {
                transform.position = blinkTarget.transform.TransformPoint(blinkTarget.Collider.offset);
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Logic
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        specialAttackTarget = MainGameUtils.FindClosestEnemyCircle(this, transform.position, SpecialAttackTeleportRange);
        blinkTarget = MainGameUtils.FindClosestEnemyCircle(this, transform.position, blinkRange);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, SpecialAttackTeleportRange);
        MainGameUtils.OnDrawGizmos_TeleportTarget(this, specialAttackTarget);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, blinkRange);
        MainGameUtils.OnDrawGizmos_TeleportTarget(this, blinkTarget, 1);
    }
}