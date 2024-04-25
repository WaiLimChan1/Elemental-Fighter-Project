using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using Unity.VisualScripting;

public class GameManager : NetworkBehaviour 
{
    //---------------------------------------------------------------------------------------------------------------------------------------------
    //GameManager Enums
    public enum GameState
    {
        PRE_ROUND, //Before Round Starts
        ROUND, //During Round
        ROUND_RESULTS, //Round Results
        ITEM_SELECTION //Selecting Items
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
    //---------------------------------------------------------------------------------------------------------------------------------------------



    //---------------------------------------------------------------------------------------------------------------------------------------------
    //Networked Variables
    [Header("Networked Variables")]
    [Networked] [SerializeField] public GameState GameStateNetworked { get; set; }
    [Networked] [SerializeField] public int GameStateIndex { get; set; }
    [Networked] [SerializeField] public int Round { get; set; }
    [Networked] [SerializeField] public bool StopChampionTakeInput { get; set; }
    [Networked] [SerializeField] TickTimer RemainingTime { get; set; }
    public float getRemainingTime()
    {
        if (Object == default) return 0;
        if (RemainingTime.ExpiredOrNotRunning(Runner)) return 0;
        return (float) RemainingTime.RemainingTime(Runner);
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
            if (NetworkedPlayerList[i] == null || NetworkedPlayerList[i].Object == default) continue;
            if (NetworkedPlayerList[i].OwnedChampion == null) continue;
            if (NetworkedPlayerList[i].OwnedChampion.GetComponent<Champion>() == null) continue;
            if (NetworkedPlayerList[i].OwnedChampion.GetComponent<Champion>().healthNetworked > 0)
            {
                alivePlayers++;
            }
        }
        return alivePlayers;
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
        RemainingTime = TickTimer.None;
    }
    //---------------------------------------------------------------------------------------------------------------------------------------------




    public void BeginPreround()
    {
        GameStateNetworked = GameState.PRE_ROUND;

        Round++;
        StopChampionTakeInput = true;
        RemainingTime = TickTimer.CreateFromSeconds(Runner, PreRoundDuration);

        GameStateIndex++;
    }

    public void BeginRound()
    {
        GameStateNetworked = GameState.ROUND;

        StopChampionTakeInput = false;
        RemainingTime = TickTimer.CreateFromSeconds(Runner, RoundDuration);

        GameStateIndex++;

        RoundRankingReversedHost_Clear();
    }

    public void BeginRoundResults()
    {
        CompleteRoundRankingReversedHost();
        GameStateNetworked = GameState.ROUND_RESULTS;

        StopChampionTakeInput = true;
        RemainingTime = TickTimer.CreateFromSeconds(Runner, RoundResultDuration);
        GameStateIndex++;
    }

    public void HandleGameStateLogic()
    {
        if (!Runner.IsServer) return;
        if (!RemainingTime.ExpiredOrNotRunning(Runner)) return;
        
        if (GameStateIndex == 0) BeginPreround();
        else if (GameStateIndex == 1) BeginRound();
        else if (GameStateIndex == 2) BeginRoundResults();
    }

    public void EndRoundEarly()
    {
        if (!Runner.IsServer) return;
        if (GameStateNetworked != GameState.ROUND) return;
        if (GetNumOfAliveChampion() >= 2) return;

        RemainingTime = TickTimer.None;
    }

    public override void FixedUpdateNetwork()
    {
        GetAllNetworkedPlayers();
        HandleGameStateLogic();
        EndRoundEarly();
    }
}
