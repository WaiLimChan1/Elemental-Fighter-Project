using Fusion;
using UnityEngine;
using static Ability;

public class Ability : NetworkBehaviour
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Static Projectile Spawn Functions
    public static void SpawnAbility(NetworkRunner Runner, Champion owner, bool isFacingLeft,
                                        NetworkPrefabRef AbilityPrefab, Vector2 SpawnPoint, float damage, AbilityStatus abilityStatus)
    {
        var Ability = Runner.Spawn(AbilityPrefab, SpawnPoint, Quaternion.identity);
        Ability.GetComponent<Ability>().SetUp(owner, damage, isFacingLeft, abilityStatus);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Ability Enums
    public enum AbilityStatus
    {
        Ability_Icon = -1,
        Leaf_Ranger_ATK3
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Ability Components
    [Header("Componets")]
    private AbilityAnimationController AbilityAnimationController;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Ability Variables
    [Header("Ability Variables")]
    [SerializeField][Networked] private Champion owner { get; set; }
    [SerializeField][Networked] private float damage { get; set; }
    [SerializeField][Networked] private bool isFacingLeft { get; set; }
    [SerializeField][Networked] private int abilityStatusNum { get; set; }

    [Header("Ability Attack Variables")]
    [SerializeField] public Transform AttackBoxesParent;
    [SerializeField] protected BoxCollider2D[] AttackBoxes;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Ability Initialization
    public override void Spawned()
    {
        AbilityAnimationController = GetComponentInChildren<AbilityAnimationController>();
    }

    public void SetUp(Champion owner, float damage, bool isFacingLeft, AbilityStatus abilityStatus)
    {
        this.owner = owner;
        this.damage = damage;
        this.isFacingLeft = isFacingLeft;
        this.abilityStatusNum = (int)abilityStatus;

        AbilityAnimationController.Flip(this.isFacingLeft);
        AbilityAnimationController.ChangeAnimation(abilityStatusNum);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public virtual int GetAttackBoxIndex()
    {
        return abilityStatusNum;
    }

    public virtual void GetAttackBoxAndDamage(ref BoxCollider2D attackBox, ref float damage, int index)
    {
        attackBox = AttackBoxes[index];
        damage = this.damage;
    }
    public virtual void AnimationTriggerAttack()
    {
        int index = GetAttackBoxIndex();
        if (index == -1) return;

        BoxCollider2D attackBox = default;
        float damage = 0;

        GetAttackBoxAndDamage(ref attackBox, ref damage, index);

        Collider2D[] colliders = Physics2D.OverlapBoxAll(attackBox.bounds.center, attackBox.bounds.size, 0, LayerMask.GetMask("Champion"));
        foreach (Collider2D collider in colliders)
        {
            Champion enemy = collider.GetComponent<Champion>();
            if (enemy != null && owner != null && enemy != owner && enemy.healthNetworked > 0)
                owner.DealDamageToVictim(enemy, damage);
        }
    }

    public virtual void AnimationTriggerCrowdControl() { }

    public virtual void AnimationTriggerDespawn()
    {
        Runner.Despawn(this.Object);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------


    public override void FixedUpdateNetwork()
    {
        if (AbilityAnimationController.AnimationFinished())
        {
            Runner.Despawn(this.Object);
            return;
        }

        //Attack Boxes And Crowd Control Boxes Flip
        if (isFacingLeft) AttackBoxesParent.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        else AttackBoxesParent.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        AbilityAnimationController.Flip(isFacingLeft);
        AbilityAnimationController.ChangeAnimation(abilityStatusNum);
    }
}
