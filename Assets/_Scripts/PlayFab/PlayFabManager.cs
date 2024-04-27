using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;
using System;


public class PlayFabManager : MonoBehaviour
{
    static public PlayerLifeTimeData PlayerLifeTimeData = new PlayerLifeTimeData();
    static public PlayerMatchData PlayerMatchData = new PlayerMatchData();

    static public void OnError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    static public void OnDataSend(UpdateUserDataResult result)
    {
        Debug.Log("Successful data send!");
    }

    static public void SetLifeTimeDataOnRegistration()
    {
        int defaultValue = 0;
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "LifeTimeKills", defaultValue.ToString() },
                { "LifeTimeDamageDealt", defaultValue.ToString() },
                { "LifeTimeDamageTaken", defaultValue.ToString() },
            }
        };

        // Call the PlayFab API to update user data
        PlayFabClientAPI.UpdateUserData(request, OnDataSend, OnError);
    }


    //----------------------------------------------------------------------------------------
    //Get LifeTime Data
    static public void GetLocalPlayerLifeTimeData(Action<GetUserDataResult> SuccessFunction, Action<PlayFabError> ErrorFunction)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), SuccessFunction, ErrorFunction);
    }

    static public bool LifeTimeDataUsable(GetUserDataResult result)
    {
        return (result.Data != null &&
            result.Data.ContainsKey("LifeTimeKills") &&
            result.Data.ContainsKey("LifeTimeDamageDealt") &&
            result.Data.ContainsKey("LifeTimeDamageTaken"));
    }

    static public void OnLifeTimeDataRecieved(GetUserDataResult result)
    {
        Debug.Log("Recieved Life Time Data!");
        if (LifeTimeDataUsable(result))
        {
            PlayerLifeTimeData.LifeTimeKills = int.Parse(result.Data["LifeTimeKills"].Value);
            PlayerLifeTimeData.LifeTimeDamageDealt = float.Parse(result.Data["LifeTimeDamageDealt"].Value);
            PlayerLifeTimeData.LifeTimeDamageTaken = float.Parse(result.Data["LifeTimeDamageTaken"].Value);
        }
    }
    //----------------------------------------------------------------------------------------



    //----------------------------------------------------------------------------------------
    //Get Match Data
    static public void GetLocalPlayerMatchData(Action<GetUserDataResult> SuccessFunction, Action<PlayFabError> ErrorFunction)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), SuccessFunction, ErrorFunction);
    }

    static public bool MatchDataUsable(GetUserDataResult result)
    {
        return (result.Data != null &&
            result.Data.ContainsKey("TotalKills") &&
            result.Data.ContainsKey("TotalDamageDealt") &&
            result.Data.ContainsKey("TotalDamageTaken") &&
            //result.Data.ContainsKey("PlayerInGameName") &&
            result.Data.ContainsKey("ChampionSelectionIndex") &&
            result.Data.ContainsKey("MatchRanking") &&
            result.Data.ContainsKey("GamePoints") &&
            result.Data.ContainsKey("ItemIndexes"));
    }

    static public void OnMatchDataRecieved(GetUserDataResult result)
    {
        Debug.Log("Recieved Match Data!");
        if (MatchDataUsable(result))
        {
            PlayerMatchData.TotalKills = int.Parse(result.Data["TotalKills"].Value);
            PlayerMatchData.TotalDamageDealt = float.Parse(result.Data["TotalDamageDealt"].Value);
            PlayerMatchData.TotalDamageTaken = float.Parse(result.Data["TotalDamageTaken"].Value);

            //PlayerMatchData.PlayerInGameName = result.Data["PlayerInGameName"].Value;
            PlayerMatchData.ChampionSelectionIndex = int.Parse(result.Data["ChampionSelectionIndex"].Value);

            PlayerMatchData.MatchRanking = int.Parse(result.Data["MatchRanking"].Value);
            PlayerMatchData.GamePoints = float.Parse(result.Data["GamePoints"].Value);
            PlayerMatchData.ItemIndexes = result.Data["ItemIndexes"].Value;
        }
    }
    //----------------------------------------------------------------------------------------



    //----------------------------------------------------------------------------------------
    //Upload new LifeTime data and Match Data
    static public void UploadLocalPlayerMatchData(Action<UpdateUserDataResult> SuccessFunction)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "LifeTimeKills", PlayerLifeTimeData.LifeTimeKills.ToString() },
                { "LifeTimeDamageDealt", PlayerLifeTimeData.LifeTimeDamageDealt.ToString() },
                { "LifeTimeDamageTaken", PlayerLifeTimeData.LifeTimeDamageTaken.ToString() },

                { "TotalKills", PlayerMatchData.TotalKills.ToString() },
                { "TotalDamageDealt", PlayerMatchData.TotalDamageDealt.ToString() },
                { "TotalDamageTaken", PlayerMatchData.TotalDamageTaken.ToString() },

                //{ "PlayerInGameName", PlayerMatchData.PlayerInGameName.ToString() },
                { "ChampionSelectionIndex", PlayerMatchData.ChampionSelectionIndex.ToString() },

                { "MatchRanking", PlayerMatchData.MatchRanking.ToString() },
                { "GamePoints", PlayerMatchData.GamePoints.ToString() },
                { "ItemIndexes", PlayerMatchData.ItemIndexes.ToString() }
            }
        };

        // Call the PlayFab API to update user data
        PlayFabClientAPI.UpdateUserData(request, SuccessFunction, OnError);
    }
    //----------------------------------------------------------------------------------------
}
