using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafElemental : ElementalChampion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Leaf Elemental Variables")]
    [SerializeField] protected BoxCollider2D SpecialAttackPullCrowdControlBox;
    [SerializeField] protected float SpecialAttackPullStrength = 10f;
    private const float SpecialAttackPullCutOffTime = 27.0f / 35.0f;

    [SerializeField] protected BoxCollider2D TransformAttackCrowdControlBox;
    [SerializeField] protected float TransformAttackKnockUpStrength = 20.0f;
    private const float TransformAttackKnockUpCutOffTime = 26.0f / 36.0f;

    [Header("Dart Variables")]
    [SerializeField] private NetworkPrefabRef DartPrefab;
    [SerializeField] private Transform DartSpawnPoint;
    [SerializeField] private float DartSpeed = 40;
    [SerializeField] private float DartLifeTime = 5;

    [Header("Javelin Variables")]
    [SerializeField] private NetworkPrefabRef JavelinPrefab;
    [SerializeField] private Transform JavelinSpawnPoint;
    [SerializeField] private float JavelinSpeed = 45;
    [SerializeField] private float JavelinLifeTime = 5;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Status Logic
    protected override bool UnstoppableStatusNetworked()
    {
        return (base.UnstoppableStatusNetworked() || statusNetworked == Status.ATTACK3);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void DealDamageToVictim(Champion enemy, float damage)
    {
        if (statusNetworked == Status.AIR_ATTACK)
            enemy.TakeDamageNetworked(this, damage, isFacingLeftNetworked, AttackType.BlockByFacingAttacker, transform.position);
        else enemy.TakeDamageNetworked(this,damage, isFacingLeftNetworked, AttackType.BlockByFacingAttack);
    }

    public override void GetControlBoxAndStrength(ref BoxCollider2D crowdControlBox, ref float crowdControlStrength, int index)
    {
        if (statusNetworked == Status.SPECIAL_ATTACK && ChampionAnimationController.GetNormalizedTime() < SpecialAttackPullCutOffTime)
        {
            crowdControlBox = SpecialAttackPullCrowdControlBox;
            crowdControlStrength = SpecialAttackPullStrength;
        }
        else if (statusNetworked == Status.UNIQUE1 && ChampionAnimationController.GetNormalizedTime() < TransformAttackKnockUpCutOffTime)
        {
            crowdControlBox = TransformAttackCrowdControlBox;
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

        if (statusNetworked == Status.AIR_ATTACK)
        {
            BoxCollider2D crowdControlBox = Attacks[0].hitBox;
            Vector2 center = AttackBoxesParent.TransformPoint(crowdControlBox.offset);

            if (enemy.transform.position.x > center.x) enemy.SetVelocity(new Vector2(crowdControlStrength, crowdControlStrength));
            else if (enemy.transform.position.x < center.x) enemy.SetVelocity(new Vector2(-1 * crowdControlStrength, crowdControlStrength));
        }
        else if (statusNetworked == Status.ATTACK3) enemy.AddVelocity(new Vector2(direction * crowdControlStrength / 2, crowdControlStrength));
        else if (statusNetworked == Status.SPECIAL_ATTACK)
        {
            //Special Attack Pull
            if (statusNetworked == Status.SPECIAL_ATTACK && ChampionAnimationController.GetNormalizedTime() < SpecialAttackPullCutOffTime)
            {
                BoxCollider2D crowdControlBox = Attacks[4].hitBox;
                Vector2 center = AttackBoxesParent.TransformPoint(crowdControlBox.offset);

                if (enemy.transform.position.x < center.x) enemy.SetVelocity(new Vector2(crowdControlStrength, 0));
                else if (enemy.transform.position.x > center.x) enemy.SetVelocity(new Vector2(-1 * crowdControlStrength, 0));
            }
            else //Explosion
            {
                enemy.AddVelocity(new Vector2(direction * crowdControlStrength / 3, crowdControlStrength));
            }
        }
        else if (statusNetworked == Status.UNIQUE1)
        {
            if (ChampionAnimationController.GetNormalizedTime() < TransformAttackKnockUpCutOffTime) //Knock Up
                enemy.AddVelocity(new Vector2(0, TransformAttackKnockUpStrength));
            else //slam
                enemy.AddVelocity(new Vector2(direction * crowdControlStrength / 2, crowdControlStrength));
        }
    }

    public override void AnimationTriggerProjectileSpawn()
    {
        if (!Runner.IsServer) return;

        if (statusNetworked == Status.ATTACK1)
            Projectile.SpawnProjectileHorizontal(Runner, this, isFacingLeftNetworked, DartPrefab, DartSpawnPoint, DartSpeed, getCalculatedDamage(Attacks[1]), Attacks[1].crowdControlStrength, DartLifeTime);
        else if (statusNetworked == Status.ATTACK2) 
            Projectile.SpawnProjectileHorizontal(Runner, this, isFacingLeftNetworked, JavelinPrefab, JavelinSpawnPoint, JavelinSpeed, getCalculatedDamage(Attacks[2]), Attacks[2].crowdControlStrength, JavelinLifeTime);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
