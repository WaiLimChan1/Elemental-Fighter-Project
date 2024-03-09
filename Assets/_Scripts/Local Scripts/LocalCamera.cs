using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalCamera : MonoBehaviour
{
    public NetworkedPlayer NetworkedPlayer;

    private void LateUpdate()
    {
        if (NetworkedPlayer != null && NetworkedPlayer.OwnedChampion != null)
        {
            transform.position = NetworkedPlayer.OwnedChampion.GetComponent<NetworkRigidbody2D>().InterpolationTarget.gameObject.transform.position;
            transform.position = new Vector3(transform.position.x, transform.position.y, -10);
        }
    }
}
