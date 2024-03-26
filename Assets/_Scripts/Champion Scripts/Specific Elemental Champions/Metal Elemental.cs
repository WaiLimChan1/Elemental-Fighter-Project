using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalElemental : ElementalChampion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
    public override void DealDamageToVictim(Champion enemy, float damage)
    {
        if (statusNetworked == Status.SPECIAL_ATTACK || statusNetworked == Status.UNIQUE1) enemy.TakeDamageNetworked(damage, isFacingLeftNetworked, AttackType.BlockByFacingAttacker, transform.position);
        else enemy.TakeDamageNetworked(damage, isFacingLeftNetworked, AttackType.BlockByFacingAttack);
    }

    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.ATTACK3)
        {
            BoxCollider2D crowdControlBox = AttackBoxes[3];
            Vector2 center = AttackBoxesParent.TransformPoint(crowdControlBox.offset);
            Vector2 changeVector = new Vector2(center.x - enemy.transform.position.x, center.y - enemy.transform.position.y).normalized;

            enemy.AddVelocity(changeVector * crowdControlStrength);
        }
        else if (statusNetworked == Status.SPECIAL_ATTACK)
        {
            BoxCollider2D crowdControlBox = AttackBoxes[4];
            Vector2 center = AttackBoxesParent.TransformPoint(crowdControlBox.offset);

            if (enemy.transform.position.x < center.x) enemy.AddVelocity(new Vector2(-1 * crowdControlStrength, crowdControlStrength));
            else enemy.AddVelocity(new Vector2(crowdControlStrength, crowdControlStrength));
        }
        else if (statusNetworked == Status.UNIQUE1)
        {
            BoxCollider2D crowdControlBox = AttackBoxes[5];
            Vector2 center = AttackBoxesParent.TransformPoint(crowdControlBox.offset);

            if (enemy.transform.position.x < center.x) enemy.AddVelocity(new Vector2(-1 * crowdControlStrength, crowdControlStrength));
            else enemy.AddVelocity(new Vector2(crowdControlStrength, crowdControlStrength));
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}