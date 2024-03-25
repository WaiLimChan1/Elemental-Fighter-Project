using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningElemental : ElementalChampion
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Champion Variables
    [Header("Lightning Elemental Variables")]
    [SerializeField] private float attack1DashRange = 15f;
    [SerializeField][Networked] private Champion championTarget { get; set; }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Attack Logic
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
    }

    public override void AnimationTriggerMobility()
    {
        if (statusNetworked == Status.ATTACK1)
        {
            championTarget = null;
            if (championTarget == null)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attack1DashRange, LayerMask.GetMask("Champion"));
                foreach (Collider2D collider in colliders)
                {
                    Champion enemy = collider.GetComponent<Champion>();
                    if (enemy != null && enemy != this && enemy.healthNetworked > 0)
                    {
                        championTarget = enemy;
                        break;
                    }
                }
            }

            if (championTarget != null)
            {
                Vector3 changeVector = championTarget.transform.TransformPoint(championTarget.Collider.offset) - AttackBoxesParent.TransformPoint(AttackBoxes[1].offset);
                transform.position = transform.position + changeVector;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, attack1DashRange);
    }
}