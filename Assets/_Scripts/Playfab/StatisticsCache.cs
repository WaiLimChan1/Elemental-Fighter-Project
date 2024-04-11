using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatisticsCache : MonoBehaviour
{
     // This object is used to help save playerData locally
     public string username; //Set as screenname at the beginning of a match 
     public int kills;
     public float damage;
     public int gold;
     public int placement;
     public int score;
     public string[] items = new string[6]; //All players can hold a max of 6 items
     //public Dictionary<string, int> items; // Problematic in newton?  




}
