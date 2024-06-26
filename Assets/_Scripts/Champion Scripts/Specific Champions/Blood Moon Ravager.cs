using Fusion;
using UnityEngine;

public class BloodMoonRavager : Champion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Blood Moon Ravager Variables")]
    [SerializeField][Networked] private Champion specialAttackTarget { get; set; }
    [SerializeField] private float specialAttackTeleportRange = 20f;

    [SerializeField] private Attack howl;
    [SerializeField] private float howlOmnivampIncrease = 0.1f;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Attack Variables & Attack Functions
    //Status.UNIQUE1 : Howl

    public override void SetAttack_ChampionUI(AllAttacks_ChampionUI ChampionUI)
    {
        base.SetAttack_ChampionUI(ChampionUI);
        ChampionUI.SetAttack_ChampionUI(ChampionUI.UniqueB, howl, "Q");
    }

    protected override Attack getAttack(Status status)
    {
        if (status == Status.UNIQUE1) return howl;
        return base.getAttack(status);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    //Status.UNIQUE1 : Howl
    protected override bool SingleAnimationStatus()
    {
        return (base.SingleAnimationStatus() ||
            status == Status.UNIQUE1);
    }

    protected override bool UnstoppableStatusNetworked()
    {
        return base.UnstoppableStatusNetworked() && statusNetworked != Status.SPECIAL_ATTACK;
    }

    protected override bool AttackSpeedStatus(Status status)
    {
        return (status == Status.AIR_ATTACK || status == Status.ATTACK1 || status == Status.ATTACK2 || status == Status.ATTACK3 || status == Status.SPECIAL_ATTACK);
    }

    protected override void TransformTakeInput() {}

    protected override void OnGroundTakeInput()
    {
        base.OnGroundTakeInput();
        if (Input.GetKeyDown(KeyCode.Q) && CanUseAttack(Status.UNIQUE1)) status = Status.UNIQUE1; //Howl
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void AnimationTriggerMobility()
    {
        if (statusNetworked == Status.SPECIAL_ATTACK)
        {
            if (specialAttackTarget == null) specialAttackTarget = MainGameUtils.FindClosestEnemyCircle(this, transform.position, specialAttackTeleportRange);
            if (specialAttackTarget != null)
            {
                Vector3 changeVector = specialAttackTarget.transform.TransformPoint(specialAttackTarget.Collider.offset) - AttackBoxesParent.TransformPoint(Attacks[4].hitBox.offset);
                transform.position = transform.position + changeVector;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Logic
    private void ApplyHowlEffects()
    {
        if (!Runner.IsServer) return;

        if (ChampionAnimationController.GetAnimatorStatus() != (int)statusNetworked) //Animation Changed
        {
            if (statusNetworked == Status.UNIQUE1) //Howl Stats Buff Effect
            {
                baseOmnivamp += howlOmnivampIncrease;
            }
        }
    }

    protected override void ApplyEffects()
    {
        base.ApplyEffects();
        ApplyHowlEffects();
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        specialAttackTarget = MainGameUtils.FindClosestEnemyCircle(this, transform.position, specialAttackTeleportRange);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, specialAttackTeleportRange);
        MainGameUtils.OnDrawGizmos_TeleportTarget(this, specialAttackTarget);
    }
}
