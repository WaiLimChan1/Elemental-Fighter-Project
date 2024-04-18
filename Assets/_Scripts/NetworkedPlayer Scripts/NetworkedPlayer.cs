using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Items
    const int MAX_ITEM_COUNT = 6;

    [Networked, Capacity(MAX_ITEM_COUNT)] public NetworkArray<int> ItemsNetworked => default;
    public List<int> ItemsHostRecord = new List<int>();

    public void UpdateItemsNetworked()
    {
        if (!Runner.IsServer) return;
        List<int> emptyList = Enumerable.Repeat(-1, MAX_ITEM_COUNT).ToList();
        ItemsNetworked.CopyFrom(emptyList, 0, emptyList.Count);
        ItemsNetworked.CopyFrom(ItemsHostRecord, 0, Mathf.Clamp(ItemsHostRecord.Count, 0, MAX_ITEM_COUNT));
    }

    public ItemManager.Item GetCombinedItemStats()
    {
        ItemManager.Item combinedItem = new ItemManager.Item();
        for (int i = 0; i < ItemsNetworked.Length; i++)
        {
            if (ItemsNetworked[i] > -1 && ItemsNetworked[i] < ItemManager.Instance.Items.Length) 
                combinedItem.CombineWithItem(ItemManager.Instance.Items[ItemsNetworked[i]]);
        }
        return combinedItem;
    }
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

        if (Runner.LocalPlayer == Object.InputAuthority)
            for (int i = 0; i < ItemsNetworked.Length; i++)
            {
                if (ItemsNetworked[i] > -1 && ItemsNetworked[i] < ItemManager.Instance.Items.Length)
                    Debug.Log(ItemManager.Instance.Items[i].itemName);
            }

        UpdateItemsNetworked();
    }

    public void DespawnOwnedChampion()
    {
        Runner.Despawn(OwnedChampion);
    }
}
