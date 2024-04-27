using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class LoginPanel : MonoBehaviour
{
    private StartMenuPanelHandler StartMenuPanelHandler;

    [Header("LoginPanel Components")]
    [SerializeField] private TMP_InputField EmailInput;
    [SerializeField] private TMP_InputField PasswordInput;

    [SerializeField] private Button LoginButton;
    [SerializeField] private Button RegisterAndLoginButton;

    [SerializeField] private Button LoginAsGuestButton;
    [SerializeField] private Button ResetPasswordButton;

    [SerializeField] private TextMeshProUGUI messageText;

    private void Start()
    {
        StartMenuPanelHandler = GetComponentInParent<StartMenuPanelHandler>();

        LoginButton.onClick.AddListener(ClickedLoginButton);
        RegisterAndLoginButton.onClick.AddListener(ClickedRegisterButton);
        LoginAsGuestButton.onClick.AddListener(ClickedLoginAsGuestButton);
        ResetPasswordButton.onClick.AddListener(ClickedResetPasswordButton);
    }

    public void Update()
    {
        if (PlayFabClientAPI.IsClientLoggedIn()) AlreadyLoggedIn();
    }

    public void AlreadyLoggedIn()
    {
        StartMenuPanelHandler.StartMenuPanel.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void OnError(PlayFabError error)
    {
        messageText.text = error.GenerateErrorReport();
    }



    //--------------------------------------------------------------------------------------------------------
    public void ClickedLoginButton()
    {
        messageText.text = "Logging In...";

        var request = new LoginWithEmailAddressRequest
        {
            Email = EmailInput.text,
            Password = PasswordInput.text
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    public void OnLoginSuccess(LoginResult result)
    {
        messageText.text = "Successfully Logged In!";
        AlreadyLoggedIn();
    }
    //--------------------------------------------------------------------------------------------------------



    //--------------------------------------------------------------------------------------------------------
    public void ClickedRegisterButton()
    {
        if (PasswordInput.text.Length < 6)
        {
            messageText.text = "Password Too Short!";
            return;
        }

        messageText.text = "Registering And Logging In...";

        var request = new RegisterPlayFabUserRequest
        {
            Email = EmailInput.text,
            Password = PasswordInput.text,
            RequireBothUsernameAndEmail = false
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    public void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        messageText.text = "Successfully Registered And Logged In!";
        PlayFabManager.SetLifeTimeDataOnRegistration();
        AlreadyLoggedIn();
    }
    //--------------------------------------------------------------------------------------------------------



    //--------------------------------------------------------------------------------------------------------
    public void ClickedLoginAsGuestButton()
    {
        messageText.text = "Logging In As Guest...";

        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnSuccessLoginAsGuest, OnError);
    }

    public void OnSuccessLoginAsGuest(LoginResult result)
    {
        messageText.text = "Successfully Logged In As Guest";
        AlreadyLoggedIn();
    }
    //--------------------------------------------------------------------------------------------------------



    //--------------------------------------------------------------------------------------------------------
    public void ClickedResetPasswordButton()
    {
        messageText.text = "Sending Password Reset Email...";

        var request = new SendAccountRecoveryEmailRequest
        {
            Email = EmailInput.text,
            TitleId = "32C76"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordResetSuccess, OnError);
    }

    void OnPasswordResetSuccess(SendAccountRecoveryEmailResult result)
    {
        messageText.text = "Password Reset Email Sent";
    }
    //--------------------------------------------------------------------------------------------------------
}
