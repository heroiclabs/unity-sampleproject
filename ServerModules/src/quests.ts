// Copyright 2021 The Nakama Authors & Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

const QuestsCollectionKey = 'quests';
const AddFriendQuestKey = 'add_friend';
const AddFriendQuestReward = 1000;
const AddFriendQuestNotificationCode = 1;

interface AddFriendQuest {
    done: boolean
}

function addFriendQuestInit(userId: string): nkruntime.StorageWriteRequest {
    return {
        collection: QuestsCollectionKey,
        key: AddFriendQuestKey,
        permissionRead: 1,
        permissionWrite: 0,
        value: {done: false} as AddFriendQuest,
        userId: userId,
    }
}

function getFriendQuest(nk: nkruntime.Nakama, logger: nkruntime.Logger, userId: string): nkruntime.StorageObject {
    let storageReadReq: nkruntime.StorageReadRequest = {
        collection: QuestsCollectionKey,
        key: AddFriendQuestKey,
        userId: userId,
    }

    let objects: nkruntime.StorageObject[];
    try {
        objects = nk.storageRead([storageReadReq]);
    } catch(error) {
        logger.error('storageRead error: %s', error.message);
        throw error;
    }

    if (objects.length === 0) {
        throw Error('user add_friend quest storage object not found');
    }

    return objects[0];
}

let afterAddFriendsFn: nkruntime.AfterHookFunction<void, nkruntime.AddFriendsRequest> = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: void, request: nkruntime.AddFriendsRequest) {
    let storedQuest = getFriendQuest(nk, logger, ctx.userId);
    let addFriendQuest = storedQuest.value as AddFriendQuest;

    if (!addFriendQuest.done) {
        let quest = addFriendQuestInit(ctx.userId);
        quest.value.done = true;

        try {
            nk.storageWrite([quest]);
        } catch (error) {
            logger.error('storageWrite error: %q', error);
            throw error;
        }

        // Notify that the quest was completed.
        let subject = JSON.stringify('A new friend!');
        let content = { reward: AddFriendQuestReward };
        let code = AddFriendQuestNotificationCode;
        let senderId = null; // Server sent
        let persistent = true;

        nk.notificationSend(
            ctx.userId,
            subject,
            content,
            code,
            senderId,
            persistent,
        );

        logger.info('user %s completed add_friend quest!', ctx.userId);
    }
}
