using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkedPlayer : NetworkBehaviour
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Components
    private LocalCamera LocalCamera;
    private ChampionUI ChampionUI;
    private ChampionSpawner ChampionSpawner;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Player Name
    [Networked] private NetworkString<_8> playerName { get; set; }

    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)] 
    private void RpcSetNickName(NetworkString<_8> nickName) { playerName = nickName; }
    //---------------------------------------------------------------------------------------------------------------------------------------------

    [Networked] public NetworkObject OwnedChampion { get; set; }

    public override void Spawned()
    {
        ChampionSpawner = GetComponent<ChampionSpawner>();
        if (Runner.LocalPlayer == Object.InputAuthority)
        {
            LocalCamera = Camera.main.GetComponent<LocalCamera>();
            LocalCamera.NetworkedPlayer = this;

            ChampionUI = ChampionUI.Instance;

            RpcSetNickName(GlobalManagers.Instance.NetworkRunnerController.LocalPlayerName);

            ChampionSpawner.Rpc_SpawnChampion(Runner.LocalPlayer, GlobalManagers.Instance.NetworkRunnerController.ChampionSelectionIndex);
        }
    }

    public void Update()
    {
        gameObject.name = "NetworkedPlayer: " + playerName + " " + Object.InputAuthority; //Object Name

        if (Runner.LocalPlayer == Object.InputAuthority)
        {
            if (OwnedChampion != null && OwnedChampion.GetComponent<Champion>() != null)
                ChampionUI.Champion = OwnedChampion.GetComponent<Champion>();
        }

        if (OwnedChampion != null && OwnedChampion.GetComponent<Champion>() != null)
            OwnedChampion.GetComponent<Champion>().SetPlayerNickName(playerName);
    }

    public void DespawnOwnedChampion()
    {
        Runner.Despawn(OwnedChampion);
    }
}
