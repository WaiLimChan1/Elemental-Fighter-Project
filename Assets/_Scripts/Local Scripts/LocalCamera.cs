using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalCamera : MonoBehaviour
{
    public NetworkedPlayer NetworkedPlayer;
    private float cameraSpeed = 15.0f;

    private void LateUpdate()
    {
        if (NetworkedPlayer.CanUseNetworkedPlayerOwnedChampion(NetworkedPlayer))
        {
            if (NetworkedPlayer.OwnedChampion.GetComponent<Champion>().healthNetworked <= 0)
            {
                Vector2 direction = new Vector2(0, 0);
                if (Input.GetKey(KeyCode.UpArrow)) direction.y += 1;
                if (Input.GetKey(KeyCode.DownArrow)) direction.y -= 1;
                if (Input.GetKey(KeyCode.LeftArrow)) direction.x -= 1;
                if (Input.GetKey(KeyCode.RightArrow)) direction.x += 1;

                transform.position += new Vector3(direction.x, direction.y, 0) * cameraSpeed * Time.deltaTime;

            }
            else
            {
                transform.position = NetworkedPlayer.OwnedChampion.GetComponent<NetworkRigidbody2D>().InterpolationTarget.gameObject.transform.position;
                transform.position = new Vector3(transform.position.x, transform.position.y, -10);
            }
        }
    }
}
