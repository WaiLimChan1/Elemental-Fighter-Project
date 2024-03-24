using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalElemental : ElementalChampion
{
    [Header("Crystal Elemental Variables")]
    [SerializeField] protected BoxCollider2D Attack3CrowdControlBox;

    //---------------------------------------------------------------------------------------------------------------------------------------------
    public override void ApplyCrowdControl(Champion enemy, float crowdControlStrength)
    {
        float direction = 1;
        if (isFacingLeftNetworked) direction *= -1;

        if (statusNetworked == Status.ATTACK3)
        {
            if (enemy.transform.position.x < transform.position.x) enemy.AddVelocity(new Vector2(crowdControlStrength / 2, crowdControlStrength));
            else enemy.AddVelocity(new Vector2(-1 * crowdControlStrength / 2, crowdControlStrength));
        }
        else if (statusNetworked == Status.SPECIAL_ATTACK) enemy.AddVelocity(new Vector2(0, crowdControlStrength));
        else if (statusNetworked == Status.UNIQUE1)
        {
            if (enemy.transform.position.x < transform.position.x) enemy.AddVelocity(new Vector2(-1 * crowdControlStrength, crowdControlStrength));
            else enemy.AddVelocity(new Vector2(crowdControlStrength, crowdControlStrength));
        }
    }

    public override void AnimationTriggerCrowdControl()
    {
        //if (Runner.IsServer)
        {
            int index = 0;
            if (statusNetworked == Status.AIR_ATTACK) index = 0;
            else if (statusNetworked == Status.ATTACK1) index = 1;
            else if (statusNetworked == Status.ATTACK2) index = 2;
            else if (statusNetworked == Status.ATTACK3) index = 3;
            else if (statusNetworked == Status.SPECIAL_ATTACK) index = 4;
            else if (statusNetworked == Status.UNIQUE1) index = 5;
            else return;

            BoxCollider2D crowdControlBox;

            if (statusNetworked == Status.ATTACK3) crowdControlBox = Attack3CrowdControlBox;
            else crowdControlBox = AttackBoxes[index];

            float crowdControlStrength = CrowdControlStrength[index];

            Collider2D[] colliders = Physics2D.OverlapBoxAll(crowdControlBox.bounds.center, crowdControlBox.bounds.size, 0, LayerMask.GetMask("Champion"));
            foreach (Collider2D collider in colliders)
            {
                Champion enemy = collider.GetComponent<Champion>();
                if (enemy != null && enemy != this && enemy.healthNetworked > 0)
                    ApplyCrowdControl(enemy, crowdControlStrength);
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
