using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPlayer : NetworkBehaviour, IBeforeUpdate
{
    [SerializeField] private LocalCamera LocalCamera;

    private ChampionSpawner ChampionSpawner;
    [Networked] public NetworkObject OwnedChampion { get; set; }

    public override void Spawned()
    {
        ChampionSpawner = GetComponent<ChampionSpawner>();
        if (Runner.LocalPlayer == Object.InputAuthority)
        {
            LocalCamera = Camera.main.GetComponent<LocalCamera>();
            LocalCamera.NetworkedPlayer = this;

            ChampionSpawner.Rpc_SpawnChampion(Runner.LocalPlayer, GlobalManagers.Instance.NetworkRunnerController.ChampionSelectionIndex);
        }
    }

    public void BeforeUpdate()
    {
        
    }

    public void DespawnOwnedChampion()
    {
        Runner.Despawn(OwnedChampion);
    }
}
