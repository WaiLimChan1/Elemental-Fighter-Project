using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkedPlayer : NetworkBehaviour
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Static Functions
    public static bool CanUseNetworkedPlayer(NetworkedPlayer networkedPlayer) { return (networkedPlayer != null && networkedPlayer.Object != default); }
    public static bool CanUseNetworkedPlayerOwnedChampion(NetworkedPlayer networkedPlayer) 
    { 
        return (CanUseNetworkedPlayer(networkedPlayer) && 
            networkedPlayer.OwnedChampion != null && networkedPlayer.OwnedChampion != default); 
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Components
    private LocalCamera LocalCamera;
    private AllAttacks_ChampionUI AllAttacks_ChampionUI;
    private ItemInventory_ChampionUI ItemInventory_ChampionUI;
    private ChampionStats_ChampionUI Stats_ChampionUI;
    private ChampionSpawner ChampionSpawner;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Player Name
    [Networked] public NetworkString<_8> playerName { get; set; }

    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)] 
    private void RpcSetNickName(NetworkString<_8> nickName) { playerName = nickName; }

    public string GetPlayerName()
    {
        return playerName.ToString() + " " + Object.InputAuthority.PlayerId;
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Items
    public const int MAX_ITEM_COUNT = 6;

    [Networked, Capacity(MAX_ITEM_COUNT)] public NetworkArray<int> ItemsNetworked => default;
    public List<int> ItemsHostRecord = new List<int>();

    public int getNumOfItems()
    {
        int numOfItems = 0;
        for (int i = 0; i < ItemsNetworked.Length; i++)
        {
            if (ItemsNetworked[i] > -1 && ItemsNetworked[i] < ItemManager.Instance.Items.Length)
                numOfItems++; 
        }
        return numOfItems;
    }

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

    public void UpdateItemUI()
    {
        if (Runner.LocalPlayer != Object.InputAuthority) return;

        for (int i = 0; i < ItemsNetworked.Length; i++)
        {
            ItemInventory_ChampionUI.SetIndividualItem_ChampionUI(i, ItemsNetworked[i]);
        }
    }

    public void AddMissedItem()
    {
        if (Runner.LocalPlayer == Object.InputAuthority)
        {
            if (GameManager.CanUseGameManager())
            {
                if (GameManager.Instance.needToAddMissedItem)
                {
                    RPC_AddItem(Random.Range(0, ItemManager.Instance.Items.Count()));
                    GameManager.Instance.needToAddMissedItem = false;
                }
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Player Game Stats
    [Header("Lifetime Data")]
    [Networked] public int TotalKills { get; set; }
    [Networked] public float TotalDamageDealt { get; set; }
    [Networked] public float TotalDamageTaken { get; set; }

    [Header("Host Data")]
    [Networked] public int ChampionSelectionIndex { get; set; }
    [Networked] public int MatchRanking { get; set; }
    [Networked] public float GamePoints { get; set; }
    //---------------------------------------------------------------------------------------------------------------------------------------------

    [Networked] public NetworkObject OwnedChampion { get; set; }

    public override void Spawned()
    {
        ChampionSpawner = GetComponent<ChampionSpawner>();
        if (Runner.LocalPlayer == Object.InputAuthority)
        {
            LocalCamera = Camera.main.GetComponent<LocalCamera>();
            LocalCamera.NetworkedPlayer = this;

            AllAttacks_ChampionUI = AllAttacks_ChampionUI.Instance;
            ItemInventory_ChampionUI = ItemInventory_ChampionUI.Instance;
            Stats_ChampionUI = ChampionStats_ChampionUI.Instance;

            if (Testing_ChampionUI.Instance != null) Testing_ChampionUI.Instance.NetworkedPlayer = this;

            RpcSetNickName(GlobalManagers.Instance.NetworkRunnerController.LocalPlayerName);
            ChampionSpawner.Rpc_SpawnChampion(Runner.LocalPlayer, GlobalManagers.Instance.NetworkRunnerController.ChampionSelectionIndex);
        }
    }

    public void Update()
    {
        gameObject.name = "NetworkedPlayer: " + playerName + " " + Object.InputAuthority; //Object Name

        //UI Update For Direct Champion Info
        if (Runner.LocalPlayer == Object.InputAuthority)
        {
            if (OwnedChampion != null && OwnedChampion.GetComponent<Champion>() != null)
            {
                AllAttacks_ChampionUI.Champion = OwnedChampion.GetComponent<Champion>();
                Stats_ChampionUI.Champion = OwnedChampion.GetComponent<Champion>();
            }
        }

        //Player Name
        if (OwnedChampion != null && OwnedChampion.GetComponent<Champion>() != null)
            OwnedChampion.GetComponent<Champion>().SetPlayerName(playerName);

        //Update Items
        UpdateItemsNetworked();

        //Update Item UI locally
        UpdateItemUI();

        //Add missed item
        AddMissedItem();
    }

    public void DespawnOwnedChampion()
    {
        Runner.Despawn(OwnedChampion);
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Testing
    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_AddItem(int itemIndex)
    {
        ItemsHostRecord.Add(itemIndex);
    }

    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RemoveAllItems()
    {
        ItemsHostRecord.Clear();
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
