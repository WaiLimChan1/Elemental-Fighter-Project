using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChampionData : INetworkInput
{
    public Champion.Status status;
    public bool isFacingLeft;
    public float inAirHorizontalMovement;
}

