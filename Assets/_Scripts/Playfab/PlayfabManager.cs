using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro; 

using UnityEngine.UI;


public class PlayfabManager : MonoBehaviour
{

     public TMP_Text messageText; //Tells users about success/failures 
     public TMP_InputField emailInput;
     public TMP_InputField passwordInput;

     //Buttons used in the login menu 
     public Button LoginClick;
     public Button RegisterClick;
     public Button ResetPasswordClick;

     public GameObject loginMenu;
     public GameObject startMenu;

     public StatisticsCache playerCache;

     void Start()
     {
          startMenu.SetActive(false);
          loginMenu.SetActive(true);
          LoginClick.onClick.AddListener(() => LoginButton());
          RegisterClick.onClick.AddListener(() => RegisterButton());
          ResetPasswordClick.onClick.AddListener(() => ResetPasswordButton());
     }

     //Makes startMenu active only after login or registration
     public void nextMenu() {
          StartCoroutine(ActivateAfterDelay(2));
          loginMenu.SetActive(false);
          startMenu.SetActive(true);
          
     }

     public void RegisterButton()
     {
          if (passwordInput.text.Length < 6)
          {
               messageText.text = "Password too short!";
               return;
          }
          var request = new RegisterPlayFabUserRequest
          {
               //Parts of a Playfab user request object
               Email = emailInput.text,
               Password = passwordInput.text,
               RequireBothUsernameAndEmail = false
          };

          PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError); 
     } 

     void OnRegisterSuccess(RegisterPlayFabUserResult result)
     {
          messageText.text = "Registered!";
          StartCoroutine(ActivateAfterDelay(1));
          SaveAllStatistics("0", "0", "0"); //Creates data for new player
          LoginButton();
     }

     public void LoginButton()
     {
          var request = new LoginWithEmailAddressRequest
          {
               Email = emailInput.text,
               Password = passwordInput.text
          };
          PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
     }  

     void OnLoginSuccess(LoginResult result)
     {
          messageText.text = "Logged in!";
          Debug.Log("Successful login/account create!");
          nextMenu();
     }

     public void ResetPasswordButton()
     {
          var request = new SendAccountRecoveryEmailRequest
          {
               Email = emailInput.text,
               TitleId = "33451"
          };
          PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);
     }
     void OnPasswordReset(SendAccountRecoveryEmailResult result)
     {
          messageText.text = "Password reset email sent";
          Debug.Log("Password reset sent");
     }

     // Update is called once per frame
     void Login() 
     {
          var request = new LoginWithCustomIDRequest
          {
               CustomId = SystemInfo.deviceUniqueIdentifier,
               CreateAccount = true
          };

          PlayFabClientAPI.LoginWithCustomID(request, OnSuccess, OnError); 
     }

     void OnSuccess(LoginResult result)
     {
          Debug.Log("Successful login/account create!");
     }

     void OnError(PlayFabError error)
     {
          messageText.text = error.GenerateErrorReport();
          Debug.Log(error.GenerateErrorReport());
     } 

     public void SendLeaderboard(string leaderboard, int score) //Specify the leaderboard and the score to send 
     //Might need to edit to accept different number types later on 
     /*
      Current Leaderboard names: 
     "Most Kills"
      */
     {
          var request = new UpdatePlayerStatisticsRequest
          {
               Statistics = new List<StatisticUpdate>
               {
                    new StatisticUpdate
                    {
                         StatisticName = leaderboard, //specific name of leaderboard in Playfab 
                         Value = score
                    }
               }
          };
          PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError); 

     }

     void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
     {
          Debug.Log("Successful leaderboard update");
     }

     IEnumerator ActivateAfterDelay(float delay)
     {
          yield return new WaitForSeconds(delay);
     }

     //General Purpose statistics method. May delete later
     /*public void SaveStatistics(string key, string value) 
     {
          Dictionary<string, string> data = new Dictionary<string, string>();

          // Add the key-value pair
          data.Add(key, value);

          // Prepare the request to update user data
          var request = new UpdateUserDataRequest
          {
               Data = data
          };
          PlayFabClientAPI.UpdateUserData(request, OnDataSend, OnError);
     }*/

     //Used to update existing stats and create new stats for registered players
     public void SaveAllStatistics(string kills, string damage, string gold)
     {

          var request = new UpdateUserDataRequest
          {
               Data = new Dictionary<string, string>
               {
                    {"Lifetime Kills", kills},
                    {"Lifetime Damage", damage},
                    {"Lifetime Gold", gold}
               }
          };
          PlayFabClientAPI.UpdateUserData(request, OnDataSend, OnError); 
     }

     void OnDataSend(UpdateUserDataResult result)
     {
          Debug.Log("Successful user data send!");
     }

     //Writes into playerCache
     public void GetAllStatistics()
     {
          PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataReceived, OnError);
     }

     void OnDataReceived(GetUserDataResult result)
     {
          Debug.Log("Received user data!");
          playerCache.kills = int.Parse(result.Data["Lifetime Kills"].Value);
          playerCache.damage = int.Parse(result.Data["Lifetime Damage"].Value);
          playerCache.gold = int.Parse(result.Data["Lifetime Gold"].Value);
     }



     

     
}