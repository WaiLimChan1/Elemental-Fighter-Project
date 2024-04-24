using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Collections.Unicode;

public class NetworkRunnerController : MonoBehaviour, INetworkRunnerCallbacks
{
    public enum EF_GAME_MODE
    {
        _1V1,
        ARENA,
        DEV_TEST
    }

    public static String GetEFGameModeString(EF_GAME_MODE EFGameMode)
    {
        if (EFGameMode == EF_GAME_MODE._1V1) return "1v1";
        else if (EFGameMode == EF_GAME_MODE.ARENA) return "Arena";
        else if (EFGameMode == EF_GAME_MODE.DEV_TEST) return "DevTest";
        return "";
    }

    public event Action OnStartedGameRunnerConnection;
    public event Action OnPlayerJoinedGameSuccessfully;

    public string LocalPlayerName;
    public int ChampionSelectionIndex;

    public GameMode LocalGameMode;
    public string RoomCode;

    [SerializeField] private NetworkRunner networkRunnerPrefab;
    public NetworkRunner networkRunnerInstance;

    public void ShutDownRunner()
    {
        networkRunnerInstance.Shutdown();
    }

    public void SetActiveScene(string SceneName)
    {
        networkRunnerInstance.SetActiveScene(SceneName);
    }

    public async void StartGame(GameMode mode, string roomCode, EF_GAME_MODE EFGameMode = EF_GAME_MODE.DEV_TEST)
    {
        OnStartedGameRunnerConnection?.Invoke();

        if (networkRunnerInstance == null)
        {
            networkRunnerInstance = Instantiate(networkRunnerPrefab);
        }

        //Register so we will get the callbacks as well
        networkRunnerInstance.AddCallbacks(this);
        networkRunnerInstance.ProvideInput = true;

        //Session Properties
        Dictionary<string, SessionProperty> sessionProperties = new Dictionary<string, SessionProperty>();
        SessionProperty SessionPropertyEFGameMode = (int) EFGameMode;
        sessionProperties.Add("EfGameMode", SessionPropertyEFGameMode);

        int EFGameModePlayerCount = 1;
        if (EFGameMode == EF_GAME_MODE._1V1) EFGameModePlayerCount = 2;
        else if (EFGameMode == EF_GAME_MODE.ARENA) EFGameModePlayerCount = 8;
        else if (EFGameMode == EF_GAME_MODE.DEV_TEST) EFGameModePlayerCount = 8;

        var startGameArgs = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomCode,
            PlayerCount = EFGameModePlayerCount,
            SessionProperties = sessionProperties,
            SceneManager = networkRunnerInstance.GetComponent<INetworkSceneManager>()
        };


        var result = await networkRunnerInstance.StartGame(startGameArgs);

        if (result.Ok)
        {
            //great
            string SCENE_NAME = GetEFGameModeString(EFGameMode);
            if (EFGameMode == EF_GAME_MODE._1V1) SCENE_NAME = "In Game Lobby";
            else if (EFGameMode == EF_GAME_MODE.ARENA) SCENE_NAME = "In Game Lobby";
            else if (EFGameMode == EF_GAME_MODE.DEV_TEST) SCENE_NAME = GetEFGameModeString(EFGameMode);
            networkRunnerInstance.SetActiveScene(SCENE_NAME);
        }
        else
        {
            Debug.Log($"Failed to start: {result.ShutdownReason}");
        }
    }

    public void OnJoinLobby()
    {
        var clientTask = JoinLobby();
    }


    public event Action OnStartedJoiningLobby;
    public event Action OnJoinedLobbySuccessfully;
    public event Action OnJoinedLobbyUnsuccessfully;

    private async Task JoinLobby()
    {
        if (networkRunnerInstance == null)
        {
            networkRunnerInstance = Instantiate(networkRunnerPrefab);
        }

        networkRunnerInstance.AddCallbacks(this);
        networkRunnerInstance.ProvideInput = false;

        Debug.Log("JoinLobby Started");
        OnStartedJoiningLobby?.Invoke();

        string lobbyID = "Lobby";

        var result = await networkRunnerInstance.JoinSessionLobby(SessionLobby.ClientServer);

        if (result.Ok)
        {
            Debug.Log("Joined Lobby ok");
            OnJoinedLobbySuccessfully?.Invoke();
        }
        else
        {
            Debug.Log($"Unable to join lobby {lobbyID}");
            OnJoinedLobbyUnsuccessfully?.Invoke();
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("OnPlayerJoined");
        OnPlayerJoinedGameSuccessfully?.Invoke();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("OnShutdown");
        SceneManager.LoadScene("StartMenu");
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) 
    {
        Debug.Log("OnSessionListUpdated");

        if (FindFirstObjectByType<RoomList>() == null) return;
        if (!FindFirstObjectByType<RoomList>().isActiveAndEnabled) return;
        RoomList RoomBrowserList = FindFirstObjectByType<RoomList>();

        RoomBrowserList.ClearList();

        if (sessionList.Count > 0)
        {
            foreach (SessionInfo sessionInfo in sessionList)
            {
                RoomBrowserList.AddToList(sessionInfo);
            }
        }
    }

    #region INetworkRunnerCallbacks
    public void OnConnectedToServer(NetworkRunner runner) { Debug.Log("OnConnectedToServer"); }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { Debug.Log("OnConnectFailed"); }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { Debug.Log("OnConnectRequest"); }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { Debug.Log("OnCustomAuthenticationResponse"); }
    public void OnDisconnectedFromServer(NetworkRunner runner) { Debug.Log("OnDisconnectedFromServer"); }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { Debug.Log("OnHostMigration"); }
    public void OnInput(NetworkRunner runner, NetworkInput input) { Debug.Log("OnInput"); }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { Debug.Log("OnInputMissing"); }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { Debug.Log("OnPlayerLeft"); }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { Debug.Log("OnReliableDataReceived"); }
    public void OnSceneLoadDone(NetworkRunner runner) { Debug.Log("OnSceneLoadDone"); }
    public void OnSceneLoadStart(NetworkRunner runner) { Debug.Log("OnSceneLoadStart"); }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { Debug.Log("OnUserSimulationMessage"); }
    #endregion
}
