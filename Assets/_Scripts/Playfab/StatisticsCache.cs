using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StatisticsCache : MonoBehaviour // This object is used to help save playerData locally
{
     public string username; //Set as screenname at the beginning of a match 
     
     //Lifetime data
     public int kills;
     public float damage;
     public int gold; 

     //Match-specific data
     public int placement;
     public int score;
     public string[] items = new string[6]; //All players can hold a max of 6 items
     public string lastPlaytime; //datetime value converted into a string
     public string champion;
      





}
