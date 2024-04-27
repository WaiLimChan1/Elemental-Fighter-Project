using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Fusion;

//This object is used to help save player data.
public struct PlayerMatchData
{
    //Match-specific data
    public string PlayerInGameName;
    public int ChampionSelectionIndex;

    public int TotalKills;
    public float TotalDamageDealt;
    public float TotalDamageTaken;
    public int MatchRanking;
    public float GamePoints;
    public string ItemIndexes;
}
