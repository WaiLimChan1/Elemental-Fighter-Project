using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPlayer : NetworkBehaviour
{
    [SerializeField] private LocalCamera LocalCamera;
    [SerializeField] private ChampionUI ChampionUI;

    private ChampionSpawner ChampionSpawner;
    [Networked] public NetworkObject OwnedChampion { get; set; }

    public override void Spawned()
    {
        ChampionSpawner = GetComponent<ChampionSpawner>();
        if (Runner.LocalPlayer == Object.InputAuthority)
        {
            LocalCamera = Camera.main.GetComponent<LocalCamera>();
            LocalCamera.NetworkedPlayer = this;

            ChampionUI = ChampionUI.Instance;

            ChampionSpawner.Rpc_SpawnChampion(Runner.LocalPlayer, GlobalManagers.Instance.NetworkRunnerController.ChampionSelectionIndex);
        }
    }

    public void Update()
    {
        if (Runner.LocalPlayer == Object.InputAuthority)
        {
            if (OwnedChampion != null && OwnedChampion.GetComponent<Champion>() != null)
                ChampionUI.Champion = OwnedChampion.GetComponent<Champion>();
        }
    }

    public void DespawnOwnedChampion()
    {
        Runner.Despawn(OwnedChampion);
    }
}
