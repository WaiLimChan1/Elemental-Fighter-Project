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

    public override void SetAttack_ChampionUI(ChampionUI ChampionUI)
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
        return (statusNetworked == Status.BEGIN_DEFEND || statusNetworked == Status.DEFEND);
    }

    protected override void TakeInput()
    {
        base.TakeInput();

        if (dead)
        {
            return;
        }

        if (!inAir && InterruptableStatus())
        {
            if (Input.GetKeyDown(KeyCode.Q) && canUseAttack(Status.UNIQUE1)) 
                status = Status.UNIQUE1;
        }
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

    protected override void ApplyEffects()
    {
        base.ApplyEffects();

        if (!Runner.IsServer) return;

        if (ChampionAnimationController.GetAnimatorStatus() != (int)statusNetworked) //Animation Changed
        {
            if (statusNetworked == Status.UNIQUE1) //Howl Stats Buff Effect
            {
                omnivamp += howlOmnivampIncrease; 
            }
        }
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
