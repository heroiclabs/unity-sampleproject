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

using DemoGame.Scripts.DataStorage;


namespace DemoGame.Scripts.Profile
{

    /// <summary>
    /// Class deriving from <see cref="DataStorage{T}"/>. Responsible for saving <see cref="PlayerData"/>
    /// on Nakama server using the built-in storage engine.
    /// </summary>
    public class PlayerDataStorage : DataStorage<PlayerData>
    {
        #region Properties

        /// <summary>
        /// Write permissions for stored data.
        /// </summary>
        public override StorageWritePermission WritePermission => StorageWritePermission.OwnerWrite;

        /// <summary>
        /// Read permissions for stored data.
        /// </summary>
        public override StorageReadPermission ReadPermission => StorageReadPermission.PublicRead;

        /// <summary>
        /// The name of collection data saved by this class will be stored in.
        /// </summary>
        public override string StorageCollection => "personal";

        #endregion

        #region Methods

        /// <summary>
        /// The key name under which data will be stored.
        /// </summary>
        public override string StorageKey(PlayerData playerData)
        {
            return "player_data";
        }

        #endregion

    }

}