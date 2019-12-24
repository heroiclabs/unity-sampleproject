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
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using System;
using System.Threading.Tasks;
using Facebook.Unity;
using System.Linq;
using DemoGame.Scripts.Utils;

namespace DemoGame.Scripts.Session
{

    /// <summary>
    /// Manages Nakama server interaction and user session throughout the game.
    /// </summary>
    /// <remarks>
    /// Whenever a user tries to communicate with game server it ensures that their session hasn't expired. If the
    /// session is expired the user will have to reauthenticate the session and obtain a new session.
    /// </remarks>
    public class NakamaSessionManager : Singleton<NakamaSessionManager>
    {
        #region Variables

        /// <summary>
        /// IP Address of the server.
        /// For demonstration purposes, the value is set through Inspector.
        /// </summary>
        [SerializeField] private string _ipAddress = "localhost";

        /// <summary>
        /// Port behind which Nakama server can be found.
        /// The default value is 7350
        /// For demonstration purposes, the value is set through Inspector.
        /// </summary>
        [SerializeField] private int _port = 7350;

        /// <summary>
        /// Cached value of <see cref="SystemInfo.deviceUniqueIdentifier"/>.
        /// Used to authenticate this device on Nakama server.
        /// </summary>
        private string _deviceId;

        /// <summary>
        /// Used to establish connection between the client and the server.
        /// Contains a list of usefull methods required to communicate with Nakama server.
        /// Do not use this directly, use <see cref="Client"/> instead.
        /// </summary>
        private Client _client;

        /// <summary>
        /// Socket responsible for maintaining connection with Nakama server and exchanger realtime messages.
        /// Do not use this directly, use <see cref="Socket"/> instead.
        /// </summary>
        private ISocket _socket;

        #region Debug

        [Header("Debug")]
        /// <summary>
        /// If true, stored session authentication token and device id will be erased on start
        /// </summary>
        [SerializeField] private bool _erasePlayerPrefsOnStart = false;

        /// <summary>
        /// Sufix added to <see cref="_deviceId"/> to generate new device id.
        /// </summary>
        [SerializeField] private string _sufix = string.Empty;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Used to communicate with Nakama server.
        /// For the user to send and receive messages from the server, <see cref="Session"/> must not be expired.
        /// Default expiration time is 60s, but for this demo we set it to 3 weeks (1 814 400 seconds).
        /// To initialize the session, call <see cref="AuthenticateDeviceIdAsync"/> or <see cref="AuthenticateFacebookAsync"/> methods.
        /// To reinitialize expired session, call <see cref="Reauthenticate"/> method.
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// Contains all the identifying data of a <see cref="Client"/>, like User Id, linked Device IDs,
        /// linked Facebook account, username, etc.
        /// </summary>
        public IApiAccount Account { get; private set; }

        /// <summary>
        /// Used to establish connection between the client and the server.
        /// Contains a list of usefull methods required to communicate with Nakama server.
        /// </summary>
        public Client Client
        {
            get
            {
                if (_client == null)
                {
                    // "defaultkey" should be changed when releasing the app
                    // see https://heroiclabs.com/docs/install-configuration/#socket
                    _client = new Client("http",_ipAddress, _port, "defaultkey",  UnityWebRequestAdapter.Instance);
                }
                return _client;
            }
        }

        /// <summary>
        /// Socket responsible for maintaining connection with Nakama server and exchange realtime messages.
        /// </summary>
        public ISocket Socket
        {
            get
            {
                if (_socket == null)
                {
                    // Initializing socket
                    _socket = _client.NewSocket();
                }
                return _socket;
            }
        }

        /// <summary>
        /// Returns true if <see cref="Session"/> between this device and Nakama server exists.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (Session == null || Session.HasExpired(DateTime.UtcNow) == true)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoked whenever client first authorizes using DeviceId.
        /// </summary>
        public event Action OnConnectionSuccess = delegate { Debug.Log(">> Connection Success"); };

        /// <summary>
        /// Invoked whenever client first authorizes using DeviceId.
        /// </summary>
        public event Action OnNewAccountCreated = delegate { Debug.Log(">> New Account Created"); };

        /// <summary>
        /// Invoked upon DeviceId authorisation failure.
        /// </summary>
        public event Action OnConnectionFailure = delegate { Debug.Log(">> Connection Error"); };

        /// <summary>
        /// Invoked after <see cref="Disconnect"/> is called.
        /// </summary>
        public event Action OnDisconnected = delegate { Debug.Log(">> Disconnected"); };

        #endregion

        #region Mono

        /// <summary>
        /// Creates new <see cref="Nakama.Client"/> object used to communicate with Nakama server.
        /// Authenticates this device using its <see cref="SystemInfo.deviceUniqueIdentifier"/> or
        /// using Facebook account, if <see cref="IsFacebookConnected"/> is true.
        /// </summary>
        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            if (_erasePlayerPrefsOnStart == true)
            {
                PlayerPrefs.SetString("nakama.authToken", "");
                PlayerPrefs.SetString("nakama.deviceId", "");
            }

            GetDeviceId();
            //await ConnectAsync();
        }

        /// <summary>
        /// Closes Nakama session.
        /// </summary>
        protected override void OnDestroy()
        {
            Disconnect();
        }

        #endregion

        #region Authentication

        public void SetIp(string ip)
        {
            if (IsConnected == false)
            {
                _ipAddress = ip;
            }
        }

        /// <summary>
        /// Restores session or tries to establish a new one.
        /// Invokes <see cref="OnConnectionSuccess"/> or <see cref="OnConnectionFailure"/>.
        /// </summary>
        /// <returns></returns>
        public async Task<AuthenticationResponse> ConnectAsync()
        {
            AuthenticationResponse response = await RestoreTokenAsync();
            switch (response)
            {
                case AuthenticationResponse.Authenticated:
                    OnConnectionSuccess?.Invoke();
                    break;
                case AuthenticationResponse.NewAccountCreated:
                    OnNewAccountCreated?.Invoke();
                    OnConnectionSuccess?.Invoke();
                    break;
                case AuthenticationResponse.Error:
                    OnConnectionFailure?.Invoke();
                    break;
                default:
                    Debug.LogError("Unhandled response received: " + response);
                    break;
            }
            return response;
        }

        /// <summary>
        /// Restores saved Session Athentication Token if user has already authenticated with the server in the past.
        /// If it's the first time authenticating using this device id, a new account will be created.
        /// </summary>
        private async Task<AuthenticationResponse> RestoreTokenAsync()
        {
            // Restoring authentication token from player prefs
            string authToken = PlayerPrefs.GetString("nakama.authToken", null);
            if (string.IsNullOrWhiteSpace(authToken) == true)
            {
                // Token not found
                // Authenticating new session
                return await AuthenticateAsync();
            }
            else
            {
                // Restoring previous session
                Session = Nakama.Session.Restore(authToken);
                if (Session.HasExpired(DateTime.UtcNow) == true)
                {
                    // Restored session has expired
                    // Authenticating new session
                    return await AuthenticateAsync();
                }
                else
                {
                    // Session restored
                    // Getting Account info
                    Account = await GetAccountAsync();
                    if (Account == null)
                    {
                        // Account not found
                        // Creating new account
                        return await AuthenticateAsync();
                    }

                    // Creating real-time communication socket
                    bool socketConnected = await ConnectSocketAsync();
                    if (socketConnected == false)
                    {
                        return AuthenticationResponse.Error;
                    }

                    Debug.Log("Session restored with token:" + Session.AuthToken);
                    return AuthenticationResponse.Authenticated;
                }
            }
        }

        /// <summary>
        /// This method authenticates this device using local <see cref="_deviceId"/> and initializes new session
        /// with Nakama server. If it's the first time user logs in using this device, a new account will be created
        /// (calling <see cref="OnDeviceIdAccountCreated"/>). Upon successfull authentication, Account data is retrieved
        /// and real-time communication socket is connected.
        /// </summary>
        /// <returns>Returns true if every server call was successful.</returns>
        private async Task<AuthenticationResponse> AuthenticateAsync()
        {
            AuthenticationResponse response = await AuthenticateDeviceIdAsync();
            if (response == AuthenticationResponse.Error)
            {
                return AuthenticationResponse.Error;
            }

            Account = await GetAccountAsync();
            if (Account == null)
            {
                return AuthenticationResponse.Error;
            }

            bool socketConnected = await ConnectSocketAsync();
            if (socketConnected == false)
            {
                return AuthenticationResponse.Error;
            }

            StoreSessionToken();
            return response;
        }

        /// <summary>
        /// Authenticates a new session using DeviceId. If it's the first time authenticating using
        /// this device, new account is created.
        /// </summary>
        /// <returns>Returns true if every server call was successful.</returns>
        private async Task<AuthenticationResponse> AuthenticateDeviceIdAsync()
        {
            try
            {
                Session = await Client.AuthenticateDeviceAsync(_deviceId, null, false);
                Debug.Log("Device authenticated with token:" + Session.AuthToken);
                return AuthenticationResponse.Authenticated;
            }
            catch (ApiResponseException e)
            {
                if (e.StatusCode == (long)System.Net.HttpStatusCode.NotFound)
                {
                    Debug.Log("Couldn't find DeviceId in database, creating new user; message: " + e);
                    return await CreateAccountAsync();
                }
                else
                {
                    Debug.LogError("An error has occured reaching Nakama server; message: " + e);
                    return AuthenticationResponse.Error;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Counldn't connect to Nakama server; message: " + e);
                return AuthenticationResponse.Error;
            }
        }

        /// <summary>
        /// Creates new account on Nakama server using local <see cref="_deviceId"/>.
        /// </summary>
        /// <returns>Returns true if account was successfully created.</returns>
        private async Task<AuthenticationResponse> CreateAccountAsync()
        {
            try
            {
                Session = await Client.AuthenticateDeviceAsync(_deviceId, null, true);
                return AuthenticationResponse.NewAccountCreated;
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't create account using DeviceId; message: " + e);
                return AuthenticationResponse.Error;
            }
        }

        /// <summary>
        /// Connects <see cref="Socket"/> to Nakama server to enable real-time communication.
        /// </summary>
        /// <returns>Returns true if socket has connected successfully.</returns>
        private async Task<bool> ConnectSocketAsync()
        {
            try
            {
                if (_socket != null)
                {
                    await _socket.CloseAsync();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Couldn't disconnect the socket: " + e);
            }

            try
            {
                await Socket.ConnectAsync(Session);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("An error has occured while connecting socket: " + e);
                return false;
            }
        }

        /// <summary>
        /// Removes session and account from cache, logs out of Facebook and invokes <see cref="OnDisconnected"/>.
        /// </summary>
        public void Disconnect()
        {
            if (FB.IsLoggedIn == true)
            {
                FB.LogOut();
                Debug.Log("Disconnected from Facebook");
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

        #region UserInfo

        /// <summary>
        /// Receives currently logged in user's <see cref="IApiAccount"/> from server.
        /// </summary>
        public async Task<IApiAccount> GetAccountAsync()
        {
            try
            {
                IApiAccount results = await Client.GetAccountAsync(Session);
                return results;
            }
            catch (Exception e)
            {
                Debug.LogWarning("An error has occured while retrieving account: " + e);
                return null;
            }
        }

        /// <summary>
        /// Receives <see cref="IApiUser"/> info from server using user id or username.
        /// Either <paramref name="userId"/> or <paramref name="username"/> must not be null.
        /// </summary>
        public async Task<IApiUser> GetUserInfoAsync(string userId, string username)
        {
            try
            {
                IApiUsers results = await Client.GetUsersAsync(Session, new string[] { userId }, new string[] { username });
                if (results.Users.Count() != 0)
                {
                    return results.Users.ElementAt(0);
                }
                else
                {
                    Debug.LogWarning("Couldn't find user with id: " + userId);
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("An error has occured while retrieving user info: " + e);
                return null;
            }
        }

        /// <summary>
        /// Async method used to update user's username and avatar url.
        /// </summary>
        public async Task<AuthenticationResponse> UpdateUserInfoAsync(string username, string avatarUrl)
        {
            try
            {
                await Client.UpdateAccountAsync(Session, username, null, avatarUrl);
                return AuthenticationResponse.UserInfoUpdated;
            }
            catch (ApiResponseException e)
            {
                Debug.LogError("Couldn't update user info with code " + e.StatusCode + ": " + e);
                return AuthenticationResponse.Error;
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't update user info: " + e);
                return AuthenticationResponse.Error;
            }
        }

        /// <summary>
        /// Retrieves device id from player prefs. If it's the first time running this app
        /// on this device, <see cref="_deviceId"/> is filled with <see cref="SystemInfo.deviceUniqueIdentifier"/>.
        /// </summary>
        private void GetDeviceId()
        {
            if (string.IsNullOrEmpty(_deviceId) == true)
            {
                _deviceId = PlayerPrefs.GetString("nakama.deviceId");
                if (string.IsNullOrWhiteSpace(_deviceId) == true)
                {
                    // SystemInfo.deviceUniqueIdentifier is not supported in WebGL,
                    // we generate a random one instead via System.Guid
#if UNITY_WEBGL && !UNITY_EDITOR
                    _deviceId = System.Guid.NewGuid().ToString();
#else
                    _deviceId = SystemInfo.deviceUniqueIdentifier;
#endif                    
                    PlayerPrefs.SetString("nakama.deviceId", _deviceId);
                }
                _deviceId += _sufix;
            }
        }

        /// <summary>
        /// Stores Nakama session authentication token in player prefs
        /// </summary>
        private void StoreSessionToken()
        {
            if (Session == null)
            {
                Debug.LogWarning("Session is null; cannot store in player prefs");
            }
            else
            {
                PlayerPrefs.SetString("nakama.authToken", Session.AuthToken);
            }
        }

        #endregion

        #region Facebook

        /// <summary>
        /// Initializes Facebook connection.
        /// </summary>
        /// <param name="handler">Invoked after Facebook authorisation.</param>
        public void ConnectFacebook(Action<FacebookResponse> handler)
        {
            if (FB.IsInitialized == false)
            {
                FB.Init(() => InitializeFacebook(handler));
            }
            else
            {
                InitializeFacebook(handler);
            }
        }

        /// <summary>
        /// Invoked by <see cref="FB.Init(InitDelegate, HideUnityDelegate, string)"/> callback.
        /// Tries to log in using Facebook account and authenticates user with Nakama server.
        /// </summary>
        /// <param name="onSuccess">Invoked when Facebook authorisation was successful.</param>
        /// <param name="onFailure">Invoked when Facebook authorisation failed.</param>
        private void InitializeFacebook(Action<FacebookResponse> handler)
        {
            FB.ActivateApp();

            List<string> permissions = new List<string>();
            permissions.Add("public_profile");

            FB.LogInWithReadPermissions(permissions, async result =>
            {
                FacebookResponse response = await ConnectFacebookAsync(result);
                handler?.Invoke(response);
            });
        }

        /// <summary>
        /// Connects Facebook to currently logged in Nakama account.
        /// </summary>
        private async Task<FacebookResponse> ConnectFacebookAsync(ILoginResult result)
        {
            FacebookResponse response = await LinkFacebookAsync(result);
            if (response != FacebookResponse.Linked)
            {
                return response;
            }

            Account = await GetAccountAsync();
            if (Account == null)
            {
                return FacebookResponse.Error;
            }

            bool socketConnected = await ConnectSocketAsync();
            if (socketConnected == false)
            {
                return FacebookResponse.Error;
            }

            StoreSessionToken();
            return FacebookResponse.Linked;
        }

        /// <summary>
        /// Tries to authenticate this user using Facebook account. If used facebook account hasn't been found in Nakama
        /// database, creates new Nakama user account and asks user if they want to transfer their progress, otherwise
        /// connects to account linked with supplied Facebook account.
        /// </summary>
        private async Task<FacebookResponse> LinkFacebookAsync(ILoginResult result = null)
        {
            if (FB.IsLoggedIn == true)
            {
                string token = AccessToken.CurrentAccessToken.TokenString;
                try
                {
                    await Client.LinkFacebookAsync(Session, token, true);
                    return FacebookResponse.Linked;
                }
                catch (ApiResponseException e)
                {
                    if (e.StatusCode == (int)System.Net.HttpStatusCode.Conflict)
                    {
                        return FacebookResponse.Conflict;
                    }
                    else
                    {
                        Debug.LogWarning("An error has occured reaching Nakama server; message: " + e);
                        return FacebookResponse.Error;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("An error has occured while connection with Facebook; message: " + e);
                    return FacebookResponse.Error;
                }
            }
            else
            {
                if (result == null)
                {
                    Debug.Log("Facebook not logged in. Call ConnectFacebook first");
                    return FacebookResponse.NotInitialized;
                }
                else if (result.Cancelled == true)
                {
                    Debug.Log("Facebook login canceled");
                    return FacebookResponse.Cancelled;
                }
                else if (string.IsNullOrWhiteSpace(result.Error) == false)
                {
                    Debug.Log("Facebook login failed with error: " + result.Error);
                    return FacebookResponse.Error;
                }
                else
                {
                    Debug.Log("Facebook login failed with no error message");
                    return FacebookResponse.Error;
                }
            }
        }

        /// <summary>
        /// Transfers this Device Id to an already existing user account linked with Facebook.
        /// This will leave current account floating, with no real device linked to it.
        /// </summary>
        public async Task<bool> MigrateDeviceIdAsync(string facebookToken)
        {
            try
            {
                Debug.Log("Starting account migration");
                string dummyGuid = _deviceId + "-";

                await Client.LinkDeviceAsync(Session, dummyGuid);
                Debug.Log("Dummy id linked");

                ISession activatedSession = await Client.AuthenticateFacebookAsync(facebookToken, null, false);
                Debug.Log("Facebook authenticated");

                await Client.UnlinkDeviceAsync(Session, _deviceId);
                Debug.Log("Local id unlinked");

                await Client.LinkDeviceAsync(activatedSession, _deviceId);
                Debug.Log("Local id linked. Migration successfull");

                Session = activatedSession;
                StoreSessionToken();

                Account = await GetAccountAsync();
                if (Account == null)
                {
                    throw new Exception("Couldn't retrieve linked account data");
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("An error has occured while linking dummy guid to local account: " + e);
                return false;
            }
        }

        #endregion

    }

}
