using System.Collections.Generic;
using UnityEngine;


public class MainGameUtils : MonoBehaviour
{
    public static List<Champion> GetAllChampionThatCanBeAttacked(Collider2D[] colliders, Champion attacker)
    {
        List<Champion> validChampions = new List<Champion>();
        foreach (Collider2D collider in colliders)
        {
            Champion enemy = collider.GetComponent<Champion>();
            if (enemy != null && attacker != null && enemy.CanBeAttacked(attacker))
                validChampions.Add(enemy);
        }
        return validChampions;
    }

    public static Champion FindClosestEnemyCircle(Champion attacker, Vector2 point, float radius)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(point, radius, LayerMask.GetMask("Champion"));
        List<Champion> validChampions = GetAllChampionThatCanBeAttacked(colliders, attacker);

        if (validChampions.Count <= 0) return null;

        int closestIndex = 0;
        for (int i = 1; i < validChampions.Count; i++)
        {
            if (Vector3.Distance(validChampions[closestIndex].transform.position, attacker.transform.position) >
                Vector3.Distance(validChampions[i].transform.position, attacker.transform.position))
                closestIndex = i;
        }

        return validChampions[closestIndex];
    }

    public static void OnDrawGizmos_TeleportTarget(Champion attacker, Champion target, float WireSphereRadius = 2)
    {
        if (target != null)
        {
            Vector3 targetPosition = target.transform.position;
            Gizmos.DrawLine(attacker.transform.position, targetPosition);
            Gizmos.DrawWireSphere(targetPosition, WireSphereRadius);
        }
    }
}
