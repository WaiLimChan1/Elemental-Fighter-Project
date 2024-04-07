using UnityEngine;
using PlayFab;
public class GlobalManagers : MonoBehaviour
{
    [SerializeField] private GameObject parentObj;
    public static GlobalManagers Instance { get; private set; }
    [field: SerializeField] public NetworkRunnerController NetworkRunnerController { get; private set; }
    [field: SerializeField] public PlayfabManager PlayfabManager { get; private set; } //Is it ok to serialize Playfab...?

     private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(parentObj);
        }
    }
}
