/**
 * Copyright 2019 The Knights Of Unity, created by Pawel Stolarczyk
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using System;
using System.Threading.Tasks;
using Facebook.Unity;

/// <summary>
/// Class responsible for managing Nakama session throuout the app ussage.
/// Whenever an user tries to comunicate with Nakama server, it must be sure that their session hasn't expired.
/// When a session is found expired, user have to reauthenticate the session, gaining new Auth Token.
/// </summary>
public class NakamaSessionManager : Singleton<NakamaSessionManager>
{
    #region Variables
    /// <summary>
    /// IP Address of the server.
    /// For demonstration purposes, the value is set through Inspector.
    /// </summary>
    [SerializeField]
    private string _ipAddress;
    /// <summary>
    /// The number of seconds before <see cref="Session"/> expires <see cref="Reauthenticate"/> is called.
    /// Should be lower than the set Nakama session timeout (default time is 60 seconds) and greater than 0.
    /// </summary>
    [SerializeField]
    private int _reauthentication = 5;
    /// <summary>
    /// Cached value of <see cref="SystemInfo.deviceUniqueIdentifier"/>.
    /// Used to authenticate this device on Nakama server.
    /// </summary>
    private string _deviceId;
    /// <summary>
    /// If true, this client is connected to Nakama server using their Facebook acoount.
    /// Don't change this value directly, use <see cref="IsFacebookConnected"/> instead.
    /// </summary>
    private bool _isFacebookConnected;
    /// <summary>
    /// If true, <see cref="ReauthenticateDelayCoroutine"/> was called and <see cref="Reauthenticate(bool)"/>
    /// will be called periodically to ensure continuous authentication.
    /// </summary>
    private Coroutine _reauthenticationCoroutine;
    #endregion

    #region Properties
    /// <summary>
    /// Used to communicate with Nakama server.
    /// For the user to send and receive messages from the server, <see cref="Session"/> must not be expired.
    /// To initialize the session, call <see cref="AuthenticateDeviceIdAsync"/> or <see cref="AuthenticateFacebookAsync"/> methods.
    /// To reinitialize expired session, call <see cref="Reauthenticate"/> method.
    /// </summary>
    public ISession Session
    {
        get;
        private set;
    }
    /// <summary>
    /// Used to establish connection between the client and the server.
    /// Contains a list of usefull methods required to communicate with Nakama server.
    /// </summary>
    public Client Client
    {
        get;
        private set;
    }
    /// <summary>
    /// Contains all the identifying data of a <see cref="Client"/>, like User Id, linked Device IDs,
    /// linked Facebook account, username, etc.
    /// </summary>
    public IApiAccount Account
    {
        get;
        private set;
    }
    /// <summary>
    /// Returns local device id, received using <see cref="SystemInfo.deviceUniqueIdentifier"/>
    /// and cached in <see cref="_deviceId"/>.
    /// </summary>
    public string LocalDeviceId
    {
        get
        {
            if (string.IsNullOrEmpty(_deviceId) == true)
            {
                GetPlayerPrefs();
            }
            return _deviceId;
        }
    }
    /// <summary>
    /// Returns true if <see cref="Session"/> between this device and Nakama server exists.
    /// </summary>
    public bool IsConnected
    {
        get
        {
            if (Session == null || Client == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
    /// <summary>
    /// Returns true if user authenticated their account using Facebook.
    /// To authenticate with Facebook account, call <see cref="ConnectFacebook"/> method.
    /// </summary>
    public bool IsFacebookConnected
    {
        get
        {
            return _isFacebookConnected;
        }
        private set
        {
            _isFacebookConnected = value;
            PlayerPrefs.SetInt("nakama.facebookAuthentication", _isFacebookConnected == true ? 1 : 0);
        }
    }
    #endregion

    #region Events
    /// <summary>
    /// Delegate used in Most <see cref="NakamaSessionManager"/> events.
    /// </summary>
    /// <param name="client"><see cref="Client"/> used to authenticate this device.</param>
    /// <param name="session">Current <see cref="Session"/> between this device and Nakama server.</param>
    public delegate void NakamaSessionDelegate(Client client, ISession session);
    /// <summary>
    /// Invoked whenever client first authorizes using DeviceId.
    /// </summary>
    public event NakamaSessionDelegate OnConnectionSuccess = delegate { Debug.Log(">> Connected"); };
    /// <summary>
    /// Invoked upon DeviceId authorisation failure.
    /// </summary>
    public event Action OnConnectionFailure = delegate { Debug.Log(">> Connection Error"); };
    /// <summary>
    /// Invoked whenever client cannot authenticate with Nakama server.
    /// This might be due no internet connection, bad http request or some internal error.
    /// Read logs for more information about the error when it occures.
    /// </summary>
    public event Action OnAuthenticationError = delegate { Debug.Log(">> Authentication Error"); };
    /// <summary>
    /// Invoked every time authentication is successful (every time user connects to Nakama server and
    /// upon conection re-authentication.
    /// </summary>
    public event NakamaSessionDelegate OnAuthenticationSuccess =
        delegate(Client client, ISession session) { Debug.Log(">> Authentication Success; session: " + session.AuthToken); };
    /// <summary>
    /// Invoked after <see cref="Disconnect"/> is called.
    /// </summary>
    public event Action OnDisconnected = delegate { Debug.Log(">> Disconnected"); };
    /// <summary>
    /// Invoked upon first connection with Nakama server using this device.
    /// </summary>
    public event NakamaSessionDelegate OnDeviceIdAccountCreated = delegate { Debug.Log(">> New DeviceId Account"); };
    /// <summary>
    /// Invoked upon logging with new Facebook account.
    /// </summary>
    public event NakamaSessionDelegate OnFacebookAccountCreated = delegate { Debug.Log(">> New Facebook Account"); };
    #endregion

    #region Mono
    /// <summary>
    /// Creates new <see cref="Nakama.Client"/> object used to communicate with Nakama server.
    /// Authenticates this device using its <see cref="SystemInfo.deviceUniqueIdentifier"/> or
    /// using Facebook account, if <see cref="IsFacebookConnected"/> is true.
    /// </summary>
    private void Start()
    {
        // "defaultkey" should be changed when releasing app
        // see https://heroiclabs.com/docs/install-configuration/#socket
        Client = new Client("defaultkey", _ipAddress, 7350, false);

        // Loading local application settings
        GetPlayerPrefs();


        if (IsFacebookConnected == true)
        {
            ConnectFacebook(OnConnectionSuccess, OnConnectionFailure);
        }
        else
        {
            ConnectDeviceId(OnConnectionSuccess, OnConnectionFailure);
        }
    }
    /// <summary>
    /// Closes Nakama session.
    /// </summary>
    private void OnDestroy()
    {
        Disconnect();
    }
    #endregion

    #region Authentication
    /// <summary>
    /// Tries to connect to Nakama server using DeviceId, cached in <see cref="_deviceId"/>.
    /// If connection is successful, invoked <see cref="OnConnectionSuccess"/>.
    /// Invokes <see cref="ReauthenticateDelay(ISession)"/> to enable periodical session
    /// On success <see cref="OnConnectionSuccess"/> is invoked, other wise, <see cref="OnConnectionFailure"/>.
    /// </summary>
    public void ConnectDeviceId()
    {
        ConnectDeviceId(OnConnectionSuccess, OnConnectionFailure);
    }
    /// <summary>
    /// Tries to connect to Nakama server using DeviceId, cached in <see cref="_deviceId"/>.
    /// If connection is successful, invoked <see cref="OnConnectionSuccess"/>.
    /// Invokes <see cref="ReauthenticateDelay(ISession)"/> to enable periodical session 
    /// </summary>
    /// <param name="onSuccess">Invoked when DeviceId authorisation was successful.</param>
    /// <param name="onFailure">Invoked when DeviceId authorisation failed.</param>
    public async void ConnectDeviceId(NakamaSessionDelegate onSuccess, Action onFailure)
    {
        await AuthenticateDeviceIdAsync();
        if (IsConnected == true)
        {
            onSuccess.Invoke(Client, Session);
            if (_reauthenticationCoroutine == null)
            {
                _reauthenticationCoroutine = StartCoroutine(ReauthenticateDelayCoroutine());
            }
        }
        else
        {
            onFailure.Invoke();
        }
    }

    /// <summary>
    /// This method authenticates this device using <see cref="_deviceId"/> and initializes new session
    /// on Nakama server. If it's the first time user logs in using this device, a new account will be created
    /// (calling <see cref="OnDeviceIdAccountCreated"/>). On success <see cref="OnAuthenticationSuccess"/> is
    /// invoked, otherwise <see cref="OnAuthenticationError"/> is invoked. Also, if user was previously connected,
    /// method <see cref="Disconnect"/> is called.
    /// </summary>
    private async Task AuthenticateDeviceIdAsync()
    {
        bool newAccountCreated = false;
        try
        {
            Session = await Client.AuthenticateDeviceAsync(_deviceId, null, false);
        }
        catch (ApiResponseException e)
        {
            if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                try
                {
                    Debug.Log("Couldn't find DeviceId in database, creating new user; message: " + e);
                    Session = await Client.AuthenticateDeviceAsync(_deviceId, null, true);
                    newAccountCreated = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError("Couldn't create account using DeviceId; message: " + ex);
                    OnAuthenticationError.Invoke();
                    Disconnect();
                    return;
                }
            }
            else
            {
                Debug.LogError("An error has occured reaching Nakama server; message: " + e);
                OnAuthenticationError.Invoke();
                Disconnect();
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Counldn't connect to Nakama server; message: " + e);
            OnAuthenticationError.Invoke();
            Disconnect();
            return;
        }

        Account = await Client.GetAccountAsync(Session);

        if (newAccountCreated == true)
        {
            OnDeviceIdAccountCreated.Invoke(Client, Session);
        }
        OnAuthenticationSuccess.Invoke(Client, Session);
    }
    /// <summary>
    /// If current <see cref="Session"/> has expired, reauthenticates the connection with
    /// previously used authentication method (calling <see cref="AuthenticateDeviceIdAsync"/>
    /// or <see cref="AuthenticateFacebookAsync(ILoginResult)"/>).
    /// </summary>
    /// <param name="force">If true, reauthentication will be forced even
    /// if <see cref="Session.ExpireTime"/> has not elapsed yet.
    /// </param>
    public async void Reauthenticate(bool force)
    {
        if (IsConnected == false)
        {
            Debug.LogWarning("No session initialized");
            return;
        }

        if (force || Session.HasExpired(DateTime.UtcNow) == true)
        {
            if (IsFacebookConnected == true)
            {
                await AuthenticateFacebookAsync();
            }
            else
            {
                await AuthenticateDeviceIdAsync();
            }
        }
    }
    /// <summary>
    /// Coroutine forcing connection reauthentication with Nakama server after some delay based on current time,
    /// <see cref="Session.ExpireTime"/> and <see cref="_reauthentication"/>.
    /// </summary>
    private IEnumerator ReauthenticateDelayCoroutine()
    {
        Debug.Log("Started periodical authentication");
        while (Session != null)
        {
            long secondsUntilExpiration = Session.ExpireTime - DateTime.UtcNow.TotalSeconds();
            yield return new WaitForSeconds(secondsUntilExpiration - _reauthentication);

            ISession lastSession = Session;
            Reauthenticate(true);
            // Waiting until Reauthenticate method resolves acynchronously and updates Session object with new AuthToken
            yield return new WaitWhile(() => Session == lastSession);
        }
    }
    /// <summary>
    /// Retrieves the ID of this device from <see cref="SystemInfo.deviceUniqueIdentifier"/> and stores it
    /// in <see cref="PlayerPrefs"/> for later uses. This id is used to authenticate with Nakama server.
    /// Stores and retrieves <see cref="IsFacebookConnected"/> for speeding up Facebook connection process.
    /// </summary>
    private void GetPlayerPrefs()
    {
        if (string.IsNullOrEmpty(_deviceId))
        {
            _deviceId = PlayerPrefs.GetString("nakama.deviceid");
            if (string.IsNullOrEmpty(_deviceId))
            {
                _deviceId = SystemInfo.deviceUniqueIdentifier;
                PlayerPrefs.SetString("nakama.deviceid", _deviceId);
            }
        }

        int facebookAuthentication = PlayerPrefs.GetInt("nakama.facebookAuthentication", 0);
        if (facebookAuthentication == 1)
        {
            IsFacebookConnected = true;
        }
        else
        {
            IsFacebookConnected = false;
        }
    }
    /// <summary>
    /// Removes session and account from cache, logs out of Facebook and invokes <see cref="OnDisconnected"/>.
    /// This method is called whenever there is an error during authentication.
    /// Stops <see cref="ReauthenticateDelayCoroutine"/>.
    /// </summary>
    public void Disconnect()
    {
        if (FB.IsLoggedIn == true)
        {
            FB.LogOut();
            Debug.Log("Disconnected from Facebook");
        }

        IsFacebookConnected = false;

        if (_reauthenticationCoroutine != null)
        {
            StopCoroutine(_reauthenticationCoroutine);
            _reauthenticationCoroutine = null;
        }

        if (Session == null)
        {
            return;
        }
        else
        {
            Session = null;
            Account = null;

            Debug.Log("Disconnected from Nakama");
            OnDisconnected.Invoke();
        }
    }
    #endregion

    #region Facebook
    /// <summary>
    /// Initializes Facebook connection.
    /// On success <see cref="OnConnectionSuccess"/> is invoked, other wise, <see cref="OnConnectionFailure"/>.
    /// </summary>
    public void ConnectFacebook()
    {
        ConnectFacebook(OnConnectionSuccess, OnConnectionFailure);
    }
    /// <summary>
    /// Initializes Facebook connection.
    /// </summary>
    /// <param name="onSuccess">Invoked when Facebook authorisation was successful.</param>
    /// <param name="onFailure">Invoked when Facebook authorisation failed.</param>
    public void ConnectFacebook(NakamaSessionDelegate onSuccess, Action onFailure)
    {
        if (FB.IsInitialized == false)
        {
            FB.Init(() => InitializeFacebook(onSuccess, onFailure));
        }
        else
        {
            InitializeFacebook(onSuccess, onFailure);
        }
    }
    /// <summary>
    /// Invoked by <see cref="FB.Init(InitDelegate, HideUnityDelegate, string)"/> callback. Tries to log in using 
    /// Facebook account and authenticates user with Nakama server using <see cref="Client.AuthenticateFacebookAsync()"/>.
    /// </summary>
    /// <param name="onSuccess">Invoked when Facebook authorisation was successful.</param>
    /// <param name="onFailure">Invoked when Facebook authorisation failed.</param>
    private void InitializeFacebook(NakamaSessionDelegate onSuccess, Action onFailure)
    {
        FB.ActivateApp();

        List<string> permissions = new List<string>();
        permissions.Add("public_profile");

        FB.LogInWithReadPermissions(permissions, (ILoginResult result) => FacebookInitialized(result, onSuccess, onFailure));
    }
    /// <summary>
    /// Invoked by <see cref="FB.LogInWithReadPermissions(IEnumerable{string}, FacebookDelegate{ILoginResult})"/> callback.
    /// If successfully logged in, authenticates connection with Nakama server.
    /// </summary>
    /// <param name="result">The result of Facebook connection, received from <see cref="FB.LogInWithReadPermissions"/>.</param>
    /// <param name="onSuccess">Invoked when Facebook authorisation was successful.</param>
    /// <param name="onFailure">Invoked when Facebook authorisation failed.</param>
    private async void FacebookInitialized(ILoginResult result, NakamaSessionDelegate onSuccess, Action onFailure)
    {
        await AuthenticateFacebookAsync(result);
        if (FB.IsLoggedIn == true && IsConnected == true)
        {
            onSuccess?.Invoke(Client, Session);
            if (_reauthenticationCoroutine == null)
            {
                _reauthenticationCoroutine = StartCoroutine(ReauthenticateDelayCoroutine());
            }
        }
        else
        {
            onFailure?.Invoke();
        }
    }
    /// <summary>
    /// Tries to authenticate this user using Facebook account. If used facebook account hasn't been found in Nakama
    /// database, creates new Nakama user account and asks user if they want to transfer their progress, otherwise
    /// connects to account linked with supplied Facebook account.
    /// </summary>
    private async Task AuthenticateFacebookAsync(ILoginResult result = null)
    {
        if (FB.IsLoggedIn == true)
        {
            bool newAccountCreated = false;
            string token = AccessToken.CurrentAccessToken.TokenString;
            try
            {
                Session = await Client.AuthenticateFacebookAsync(token, null, false);
            }
            catch (ApiResponseException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    try
                    {
                        Debug.Log("Couldn't find Facebook account in database, creating new user; message: " + e);
                        Session = await Client.AuthenticateFacebookAsync(token, null, true);
                        newAccountCreated = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Couldn't create account using Facebook; message: " + ex);
                        OnAuthenticationError.Invoke();
                        Disconnect();
                        return;
                    }
                }
                else
                {
                    Debug.LogError("An error has occured reaching Nakama server; message: " + e);
                    OnAuthenticationError.Invoke();
                    Disconnect();
                    return;
                }
            }

            Account = await Client.GetAccountAsync(Session);
            IsFacebookConnected = true;

            if (newAccountCreated == true)
            {
                OnFacebookAccountCreated.Invoke(Client, Session);
            }
            OnAuthenticationSuccess.Invoke(Client, Session);
        }
        else
        {
            if (result == null)
            {
                Debug.Log("Facebook not logged in. Call ConnectFacebook first");
            }
            else if (result.Cancelled == true)
            {
                Debug.Log("Facebook login canceled");
            }
            else if (string.IsNullOrWhiteSpace(result.Error) == false)
            {
                Debug.Log("Facebook login failed with error: " + result.Error);
            }
            IsFacebookConnected = false;
        }
    }
    #endregion

}
