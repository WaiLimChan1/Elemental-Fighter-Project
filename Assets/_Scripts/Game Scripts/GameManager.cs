using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : NetworkBehaviour
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //GameManager Enums
    public enum GameState
    {
        PRE_ROUND, //Before Round Starts
        ROUND, //During Round
        ROUND_RESULTS, //Round Results
        ITEM_SELECTION, //Selecting Items
        MATCH_RESULTS //Match Results
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //GameManager Static
    public static GameManager Instance;
    public static bool CanUseGameManager()
    {
        return GameManager.Instance != null && GameManager.Instance.Object != default;
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Game Mode Variables
    [Header("Game Mode Variables")]
    [SerializeField] float PreRoundDuration = 3f;
    [SerializeField] float RoundDuration = 120f;
    [SerializeField] float RoundResultDuration = 10f;
    [SerializeField] float ItemSelectionDuration = 10f;

    [SerializeField] GameState[] GameModeGameStates;

    [SerializeField] float[] PointsForRoundRanking;

    [SerializeField] Transform[] SpawnPositions;
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Networked Variables
    [Header("Networked Variables")]
    [Networked][SerializeField] public GameState GameStateNetworked { get; set; }
    [Networked][SerializeField] public int GameStateIndex { get; set; }
    [Networked][SerializeField] public int Round { get; set; }
    [Networked][SerializeField] public bool StopChampionTakeInput { get; set; }
    [Networked][SerializeField] public bool StopResourceRegenAndDecay { get; set; }
    [Networked][SerializeField] TickTimer RemainingTime { get; set; }
    public float getRemainingTime()
    {
        if (Object == default) return 0;
        if (RemainingTime.ExpiredOrNotRunning(Runner)) return 0;
        return (float)RemainingTime.RemainingTime(Runner);
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //NetworkedPlayer Round Reverse Ranking (Index 0 is last place. Highest index is first place)
    public const int MAX_PLAYER_COUNT = 8;

    [Networked, Capacity(MAX_PLAYER_COUNT)] public NetworkArray<NetworkedPlayer> RoundRankingReversedNetworked => default;
    public List<NetworkedPlayer> RoundRankingReversedHost = new List<NetworkedPlayer>();

    public void UpdateRoundRankingReverseNetworked()
    {
        if (!Runner.IsServer) return;

        List<NetworkedPlayer> emptyList = Enumerable.Repeat<NetworkedPlayer>(null, MAX_PLAYER_COUNT).ToList();
        RoundRankingReversedNetworked.CopyFrom(emptyList, 0, emptyList.Count);
        RoundRankingReversedNetworked.CopyFrom(RoundRankingReversedHost, 0, Mathf.Clamp(RoundRankingReversedHost.Count, 0, MAX_PLAYER_COUNT));
    }

    public void RoundRankingReversedHost_Clear()
    {
        if (!Runner.IsServer) return;

        RoundRankingReversedHost.Clear();
        UpdateRoundRankingReverseNetworked();
    }

    public void RoundRankingReversedHost_Add(NetworkedPlayer networkedPlayer)
    {
        if (!Runner.IsServer) return;

        RoundRankingReversedHost.Add(networkedPlayer);
        UpdateRoundRankingReverseNetworked();
    }

    //Find all of the remaining networked player not yet in RoundRankingReversedHost
    //Sort RoundRankingReversedHost in reverse order (lowest health added first).
    //Add everything from RemainingNetworkedPlayers list into RoundRankingReversedHost
    public void CompleteRoundRankingReversedHost()
    {
        if (!Runner.IsServer) return;


        //Find all of the remaining networked players not yet in RoundRankingReversedHost
        GetAllNetworkedPlayers();
        List<NetworkedPlayer> RemainingNetworkedPlayers = new List<NetworkedPlayer>();
        for (int i = 0; i < NetworkedPlayerList.Count; i++)
            if (!RoundRankingReversedHost.Contains(NetworkedPlayerList[i]))
                RemainingNetworkedPlayers.Add(NetworkedPlayerList[i]);

        //Sort RoundRankingReversedHost in reverse order (lowest health added first).
        for (int i = 0; i < RemainingNetworkedPlayers.Count - 1; i++)
        {
            int lowestIndex = i;
            for (int j = i + 1; j < RemainingNetworkedPlayers.Count; j++)
            {
                if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedChampion(RemainingNetworkedPlayers[lowestIndex]) ||
                    !NetworkedPlayer.CanUseNetworkedPlayerOwnedChampion(RemainingNetworkedPlayers[j]))
                    continue;

                if (RemainingNetworkedPlayers[lowestIndex].OwnedChampion.GetComponent<Champion>().healthNetworked >
                    RemainingNetworkedPlayers[j].OwnedChampion.GetComponent<Champion>().healthNetworked)
                {
                    lowestIndex = j;
                }

            }

            if (lowestIndex != i)
            {
                NetworkedPlayer temp = RemainingNetworkedPlayers[i];
                RemainingNetworkedPlayers[i] = RemainingNetworkedPlayers[lowestIndex];
                RemainingNetworkedPlayers[lowestIndex] = temp;
            }
        }

        //Add everything from RemainingNetworkedPlayers list into RoundRankingReversedHost
        foreach (NetworkedPlayer networkedPlayer in RemainingNetworkedPlayers)
        {
            if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedChampion(networkedPlayer)) continue;
            RoundRankingReversedHost_Add(networkedPlayer);
        }
    }

    public void CalculateRoundPoints()
    {
        if (!Runner.IsServer) return;

        int RoundRanking = 0;
        for (int i = RoundRankingReversedHost.Count - 1; i >= 0; i--)
        {
            NetworkedPlayer currentNetworkedPlayer = RoundRankingReversedHost[i];
            if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedChampion(currentNetworkedPlayer)) continue;
            if (RoundRanking >= PointsForRoundRanking.Length) continue;

            currentNetworkedPlayer.GamePoints += PointsForRoundRanking[RoundRanking];
            RoundRanking++;
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------




    //---------------------------------------------------------------------------------------------------------------------------------------------
    //NetworkedPlayer List
    [SerializeField] public List<NetworkedPlayer> NetworkedPlayerList = new List<NetworkedPlayer>();

    public void GetAllNetworkedPlayers()
    {
        NetworkedPlayerList.Clear();
        foreach (PlayerRef playerRef in Runner.ActivePlayers)
        {
            if (Runner.TryGetPlayerObject(playerRef, out var playerNetworkObject))
            {
                NetworkedPlayer NetworkedPlayer = playerNetworkObject.GetComponent<NetworkedPlayer>();
                if (!NetworkedPlayer.CanUseNetworkedPlayer(NetworkedPlayer)) continue;
                NetworkedPlayerList.Add(NetworkedPlayer);
            }
        }
    }

    public int GetNumOfAliveChampion()
    {
        GetAllNetworkedPlayers();

        int alivePlayers = 0;
        for (int i = 0; i < NetworkedPlayerList.Count; i++)
        {
            NetworkedPlayer current = NetworkedPlayerList[i];
            if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedChampion(current)) continue;
            if (current.OwnedChampion.GetComponent<Champion>().healthNetworked > 0)
            {
                alivePlayers++;
            }
        }
        return alivePlayers;
    }

    public void ReviveAllNetworkedPlayerChampions()
    {
        if (!Runner.IsServer) return;

        GetAllNetworkedPlayers();
        for (int i = 0; i < NetworkedPlayerList.Count; i++)
        {
            NetworkedPlayer current = NetworkedPlayerList[i];
            if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedChampion(current)) continue;
            current.OwnedChampion.GetComponent<Champion>().HostResetChampion();
        }
    }

    public void RepositionAllNetworkedPlayerChampions()
    {
        if (!Runner.IsServer) return;

        List<int> remainingIndexes = new List<int>();
        for (int i = 0; i < SpawnPositions.Length; i++) remainingIndexes.Add(i);


        GetAllNetworkedPlayers();
        for (int i = 0; i < NetworkedPlayerList.Count; i++)
        {
            NetworkedPlayer current = NetworkedPlayerList[i];
            if (!NetworkedPlayer.CanUseNetworkedPlayerOwnedChampion(current)) continue;

            int randomIndex = 0;

            if (remainingIndexes.Count() > 0)
            {
                int randomIndexOfRemainingIndexes = Random.Range(0, remainingIndexes.Count());
                randomIndex = remainingIndexes[randomIndexOfRemainingIndexes];
                remainingIndexes.RemoveAt(randomIndexOfRemainingIndexes);
            }

            current.OwnedChampion.GetComponent<Champion>().HostReposition(SpawnPositions[randomIndex]);
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------




    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Item Selection List
    [Networked] [SerializeField] int ItemCount { get; set; }

    public bool selectedItemLocal;

    public bool needToAddMissedItem;
    public int backUpItemIndexLocal;

    public const int NUM_SELECTION_ITEMS = 5;
    [SerializeField] public List<int> ItemSelectionList = new List<int>();


    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_GenerateRandomItemSelectionList()
    {
        ItemSelectionList.Clear();
        backUpItemIndexLocal = 0;

        for (int i = 0; i < NUM_SELECTION_ITEMS; i++)
        {
            int randomItemIndex = 0;
            do
            {
                randomItemIndex = Random.Range(0, ItemManager.Instance.Items.Length);
            } while (ItemSelectionList.Contains(randomItemIndex));

            ItemSelectionList.Add(randomItemIndex);
        }

        backUpItemIndexLocal = ItemSelectionList[Random.Range(0, ItemSelectionList.Count)];
    }

    public void AddSelectedItem(int itemIndex)
    {
        if (Runner.TryGetPlayerObject(Runner.LocalPlayer, out var playerNetworkObject))
        {
            NetworkedPlayer LocalNetworkedPlayer = playerNetworkObject.GetComponent<NetworkedPlayer>();
            if (!NetworkedPlayer.CanUseNetworkedPlayer(LocalNetworkedPlayer)) return;

            LocalNetworkedPlayer.RPC_AddItem(itemIndex);

            selectedItemLocal = true;
        }
    }

    //When Item Selection ended, if a player has less item than ItemCount, have them gain an item.
    [Rpc(sources: RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_HandleMissedSelectingItem(int PreviousItemCount)
    {
        if (Runner.TryGetPlayerObject(Runner.LocalPlayer, out var playerNetworkObject))
        {
            NetworkedPlayer LocalNetworkedPlayer = playerNetworkObject.GetComponent<NetworkedPlayer>();
            if (!NetworkedPlayer.CanUseNetworkedPlayer(LocalNetworkedPlayer)) return;

            if (LocalNetworkedPlayer.getNumOfItems() < PreviousItemCount)
            {
                needToAddMissedItem = true;
            }
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Match Results
    [Header("Match Results")]
    [Networked, Capacity(MAX_PLAYER_COUNT)] public NetworkArray<NetworkString<_16>> MatchResultNamesNetworked => default;
    [Networked, Capacity(MAX_PLAYER_COUNT)] public NetworkArray<int> MatchResultPointsNetworked => default;

    public void CalculatePlayerRankByGamePoint()
    {
        if (!Runner.IsServer) return;

        GetAllNetworkedPlayers();
        for (int i = 0; i < NetworkedPlayerList.Count() - 1; i++)
        {
            int biggestIndex = i;
            for (int j = i + 1; j < NetworkedPlayerList.Count(); j++)
            {
                if (NetworkedPlayerList[biggestIndex].GamePoints < NetworkedPlayerList[j].GamePoints)
                    biggestIndex = j;
            }

            if (biggestIndex != i)
            {
                NetworkedPlayer temp = NetworkedPlayerList[i];
                NetworkedPlayerList[i] = NetworkedPlayerList[biggestIndex];
                NetworkedPlayerList[biggestIndex] = temp;
            }
        }
    }

    public void StoreMatchResultDataInNetworkArrays()
    {
        if (!Runner.IsServer) return;

        CalculatePlayerRankByGamePoint();

        List<NetworkString<_16>> emptyMatchResultNames = Enumerable.Repeat<NetworkString<_16>>("", MAX_PLAYER_COUNT).ToList();
        MatchResultNamesNetworked.CopyFrom(emptyMatchResultNames, 0, emptyMatchResultNames.Count);

        List<int> emptyMatchResultPoints = Enumerable.Repeat<int>(0, MAX_PLAYER_COUNT).ToList();
        MatchResultPointsNetworked.CopyFrom(emptyMatchResultPoints, 0, emptyMatchResultPoints.Count);

        List<NetworkString<_16>> MatchResultNamesLocal = new List<NetworkString<_16>>();
        List<int> MatchResultPointsLocal = new List<int>();

        for (int i = 0; i < NetworkedPlayerList.Count(); i++)
        {
            MatchResultNamesLocal.Add(NetworkedPlayerList[i].GetPlayerName());
            MatchResultPointsLocal.Add((int) NetworkedPlayerList[i].GamePoints);
        }

        MatchResultNamesNetworked.CopyFrom(MatchResultNamesLocal, 0, Mathf.Clamp(MatchResultNamesLocal.Count, 0, MAX_PLAYER_COUNT));
        MatchResultPointsNetworked.CopyFrom(MatchResultPointsLocal, 0, Mathf.Clamp(MatchResultPointsLocal.Count, 0, MAX_PLAYER_COUNT));
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //GameManager Initialization
    public void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }

    public override void Spawned()
    {
        if (!Runner.IsServer) return;

        GameStateNetworked = GameState.PRE_ROUND;
        GameStateIndex = 0;
        Round = 0;
        StopChampionTakeInput = false;
        StopResourceRegenAndDecay = true;
        RemainingTime = TickTimer.CreateFromSeconds(Runner, 0.5f);

        ItemCount = 0;
        selectedItemLocal = true;
        needToAddMissedItem = false;
        backUpItemIndexLocal = 0;
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Begin Game State Logic
    public void BeginPreround()
    {
        ReviveAllNetworkedPlayerChampions(); //Revive all champions
        RepositionAllNetworkedPlayerChampions(); //Reposition all champions

        GameStateNetworked = GameState.PRE_ROUND;

        Round++;
        StopChampionTakeInput = true;
        StopResourceRegenAndDecay = true;
        RemainingTime = TickTimer.CreateFromSeconds(Runner, PreRoundDuration);
    }

    public void BeginRound()
    {
        ReviveAllNetworkedPlayerChampions(); //Revive all champions

        GameStateNetworked = GameState.ROUND;

        StopChampionTakeInput = false;
        StopResourceRegenAndDecay = false;
        RemainingTime = TickTimer.CreateFromSeconds(Runner, RoundDuration);

        RoundRankingReversedHost_Clear();
    }

    public void BeginRoundResults()
    {
        CompleteRoundRankingReversedHost(); //Complete Round Ranking
        CalculateRoundPoints(); //Calculate Points

        GameStateNetworked = GameState.ROUND_RESULTS;

        StopChampionTakeInput = true;
        StopResourceRegenAndDecay = true;
        RemainingTime = TickTimer.CreateFromSeconds(Runner, RoundResultDuration);
    }

    public void BeginItemSelection()
    {
        RPC_GenerateRandomItemSelectionList(); //Everyone generate their own random item list
        GameStateNetworked = GameState.ITEM_SELECTION;

        StopChampionTakeInput = true;
        StopResourceRegenAndDecay = true;
        RemainingTime = TickTimer.CreateFromSeconds(Runner, ItemSelectionDuration);

        ItemCount++;
    }

    public void BeginMatchResults()
    {
        StoreMatchResultDataInNetworkArrays();

        GameStateNetworked = GameState.MATCH_RESULTS;

        StopChampionTakeInput = true;
        StopResourceRegenAndDecay = true;
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Game Manager Logic
    public void HandleEndGameStateLogicHost()
    {
        if (!Runner.IsServer) return;
        if (!RemainingTime.ExpiredOrNotRunning(Runner)) return;

        int currentGameStateIndex = GameStateIndex - 1;
        if (currentGameStateIndex < 0 || currentGameStateIndex >= GameModeGameStates.Length) return;

        GameState currentGameState = GameModeGameStates[currentGameStateIndex];
        if (currentGameState == GameState.ITEM_SELECTION)
        {
            RPC_HandleMissedSelectingItem(ItemCount);
        }
    }

    public void HandleBeginGameStateLogicHost()
    {
        if (!Runner.IsServer) return;
        if (!RemainingTime.ExpiredOrNotRunning(Runner)) return;
        if (GameStateIndex >= GameModeGameStates.Length) return;

        GameState newGameState = GameModeGameStates[GameStateIndex];

        if (newGameState == GameState.PRE_ROUND) BeginPreround();
        else if (newGameState == GameState.ROUND) BeginRound();
        else if (newGameState == GameState.ROUND_RESULTS) BeginRoundResults();
        else if (newGameState == GameState.ITEM_SELECTION) BeginItemSelection();
        else if (newGameState == GameState.MATCH_RESULTS) BeginMatchResults();

        GameStateIndex++;
    }

    public void HandleBeginGameStateLogicLocal()
    {
        if (!RemainingTime.ExpiredOrNotRunning(Runner)) return;
        if (GameStateIndex >= GameModeGameStates.Length) return;

        GameState newGameState = GameModeGameStates[GameStateIndex];
        
        if (newGameState == GameState.ITEM_SELECTION) selectedItemLocal = false;
    }

    public void EndRoundEarly()
    {
        if (!Runner.IsServer) return;
        if (GameStateNetworked != GameState.ROUND) return;
        if (GetNumOfAliveChampion() >= 2) return;

        RemainingTime = TickTimer.None;
    }

    public void EndMatchEarly()
    {
        if (!Runner.IsServer) return;
        if (GameStateNetworked == GameState.MATCH_RESULTS) return;

        if (Runner.ActivePlayers.Count() >= 2) return;

        RemainingTime = TickTimer.None;
        GameStateIndex = GameModeGameStates.Count() - 1;
    }

    public override void FixedUpdateNetwork()
    {
        GetAllNetworkedPlayers();

        HandleEndGameStateLogicHost();

        HandleBeginGameStateLogicLocal();
        HandleBeginGameStateLogicHost();

        EndRoundEarly();
        EndMatchEarly();
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------
}
