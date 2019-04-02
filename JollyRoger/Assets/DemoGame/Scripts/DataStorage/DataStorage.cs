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

using System;
using System.Linq;
using System.Threading.Tasks;
using DemoGame.Scripts.Session;
using Nakama;
using UnityEngine;

namespace DemoGame.Scripts.DataStorage
{

    /// <summary>
    /// Abstract class used to store and retrieve data from Nakama server.
    /// This class utilizes Nakama's Storage Engine, documentation of which
    /// can be found here: https://heroiclabs.com/docs/storage-collections/.
    /// </summary>
    /// <typeparam name="T">The data model to be serialized into Json and stored on Nakama server.</typeparam>
    public abstract class DataStorage<T> : MonoBehaviour where T : class
    {
        #region Properties

        /// <summary>
        /// The name of collection data handled by this class will be stored in.
        /// </summary>
        public abstract string StorageCollection { get; }

        /// <summary>
        /// Determines who can change value of our data object on the server. 
        /// </summary>
        public abstract StorageWritePermission WritePermission { get; }

        /// <summary>
        /// Determines who can read value of our data object on the server.
        /// </summary>
        public abstract StorageReadPermission ReadPermission { get; }

        #endregion

        #region Methods

        /// <summary>
        /// The key name under which data handled by this class can be found.
        /// </summary>
        public abstract string StorageKey(T data);

        /// <summary>
        /// Converts given data into Json string and sends it to Nakama server.
        /// </summary>
        public virtual async Task<bool> StoreDataAsync(T data)
        {
            string json = Nakama.TinyJson.JsonWriter.ToJson(data);
            string key = StorageKey(data);
            return await StoreDataAsync(key, json);
        }

        /// <summary>
        /// Stores given <paramref name="data"/> under specified <paramref name="key"/> in Nakama storage system.
        /// </summary>
        public virtual async Task<bool> StoreDataAsync(string key, string data)
        {
            Client client = NakamaSessionManager.Instance.Client;
            ISession session = NakamaSessionManager.Instance.Session;

            WriteStorageObject storageObject = new WriteStorageObject();
            storageObject.Collection = StorageCollection;
            storageObject.Key = key;
            storageObject.Value = data;
            storageObject.PermissionRead = (int)ReadPermission;
            storageObject.PermissionWrite = (int)WritePermission;
            try
            {
                // Method Client.WriteStorageObjectsAsync allows us to send multiple WriteStorageObjects at once, but in this
                // demo we only ever need to send one at the time
                IApiStorageObjectAcks sentObjects = await client.WriteStorageObjectsAsync(session, storageObject);
                Debug.Log("Successfully send " + typeof(T).Name + " data: " + (sentObjects.Acks.Count() == 1 ? "true" : "false"));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("An exception has occured while sending user data: " + e);
                return false;
            }
        }

        /// <summary>
        /// Sends request to read data from the server and converts it from Json to <typeparamref name="T"/>.
        /// Uses local <see cref="NakamaSessionManager.Account"/> as target user we want to get data of.
        /// </summary>
        public virtual async Task<T> LoadDataAsync(string key)
        {
            return await LoadDataAsync(NakamaSessionManager.Instance.Account.User.Id, key);
        }

        /// <summary>
        /// Sends request to read data from the server and converts it from Json to <typeparamref name="T"/>.
        /// </summary>
        public virtual async Task<T> LoadDataAsync(string userId, string key)
        {
            string json = await LoadDataJsonAsync(userId, key);
            if (json == null)
            {
                Debug.Log("Couldn't retrieve data with key " + key);
                return null;
            }
            else
            {
                T obj = Nakama.TinyJson.JsonParser.FromJson<T>(json);
                return obj;
            }
        }

        /// <summary>
        /// Sends request to read data from the server and returns json.
        /// </summary>
        public virtual async Task<string> LoadDataJsonAsync(string userId, string key)
        {
            Client client = NakamaSessionManager.Instance.Client;
            ISession session = NakamaSessionManager.Instance.Session;

            StorageObjectId storageObject = new StorageObjectId();
            storageObject.Collection = StorageCollection;
            storageObject.Key = key;
            storageObject.UserId = userId;

            try
            {
                IApiStorageObjects receivedObjects = await client.ReadStorageObjectsAsync(session, storageObject);
                // Method Client.ReadStorageObjectsAsync returns an enumerable of json strings, and because we
                // passed only a single StorageObjectId, we expect to receive only one string
                if (receivedObjects.Objects.Count() > 0)
                {
                    string json = receivedObjects.Objects.ElementAt(0).Value;
                    return json;
                }
                else
                {
                    Debug.Log("No " + typeof(T).Name + " data in " + StorageCollection + "." + key + " found for this user");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("An exception has occured while receiving user data: " + e);
                return null;
            }
        }

        #endregion
    }

}