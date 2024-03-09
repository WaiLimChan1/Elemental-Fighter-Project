using UnityEngine;
using Fusion;

public class ChampionSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef[] championPrefabs;
    [SerializeField] private NetworkedPlayer NetworkedPlayer;

    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_SpawnChampion(PlayerRef playerRef, int ChampionSelectionIndex)
    {
        var spawnPoint = new Vector3(0, 0, 0);
        ChampionSelectionIndex = Mathf.Clamp(ChampionSelectionIndex, 0, championPrefabs.Length - 1);
        var champion = Runner.Spawn(championPrefabs[ChampionSelectionIndex], spawnPoint, Quaternion.identity, playerRef);
        NetworkedPlayer.OwnedChampion = champion;
    }
}
