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

const dummyUserDeviceId = 'B1DA5988-FC6F-4B6F-8EA9-217DEEC3CDB6';
const dummyUserDeviceUsername = 'SuperPirate';

const globalLeaderboard = 'global';
const leaderboardIds = [
    globalLeaderboard,
];

/**
 * Main function.
 */
const InitModule: nkruntime.InitModule =
        function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, initializer: nkruntime.Initializer) {
    // Add at least one user to the system.
    nk.authenticateDevice(dummyUserDeviceId, dummyUserDeviceUsername, true);

    // Set up leaderboards.
    const authoritative = false;
    const metadata = {};
    const scoreOperator = nkruntime.Operator.BEST;
    const sortOrder = nkruntime.SortOrder.DESCENDING;
    const resetSchedule = null;
    leaderboardIds.forEach(id => {
        nk.leaderboardCreate(id, authoritative, sortOrder, scoreOperator, resetSchedule, metadata);
        logger.info('leaderboard %q created', id);
    });

    // Set up hooks.
    initializer.registerAfterAuthenticateDevice(afterAuthenticateDeviceFn);
    initializer.registerAfterAuthenticateFacebook(afterAuthenticateFacebookFn);
    initializer.registerAfterJoinGroup(afterJoinGroupFn);
    initializer.registerAfterKickGroupUsers(afterKickGroupUsersFn);
    initializer.registerAfterLeaveGroup(afterLeaveGroupFn);
    initializer.registerAfterPromoteGroupUsers(afterPromoteGroupUsersFn);
    initializer.registerAfterAddFriends(afterAddFriendsFn);
    initializer.registerBeforeDeleteGroup(beforeDeleteGroupFn);

    // Set up RPCs: For Pirate Panic
    initializer.registerRpc('search_username', rpcSearchUsernameFn);
    initializer.registerRpc('swap_deck_card', rpcSwapDeckCard);
    initializer.registerRpc('upgrade_card', rpcUpgradeCard);
    initializer.registerRpc('reset_card_collection', rpcResetCardCollection);
    initializer.registerRpc('add_user_gems', rpcAddUserGems);
    initializer.registerRpc('load_user_cards', rpcLoadUserCards);
    initializer.registerRpc('add_random_card', rpcBuyRandomCard);
    initializer.registerRpc('handle_match_end', rpcHandleMatchEnd);
    logger.warn('Pirate Panic TypeScript loaded.');

    //  -------------------------------------------
    //  NOTE: Set up RPCs: For Example Scene(s)
    //        Register each TypeScript method to
    //        be callable from C#
    //  -------------------------------------------
    initializer.registerRpc('AddNumbers', AddNumbers);
    logger.warn('Examples TypeScript loaded.');
}

const afterAuthenticateDeviceFn: nkruntime.AfterHookFunction<nkruntime.Session, nkruntime.AuthenticateDeviceRequest> =
        function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: nkruntime.Session, req: nkruntime.AuthenticateDeviceRequest) {
    afterAuthenticate(ctx, logger, nk, data);
}

const afterAuthenticateFacebookFn: nkruntime.AfterHookFunction<nkruntime.Session, nkruntime.AuthenticateFacebookRequest> =
        function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: nkruntime.Session, req: nkruntime.AuthenticateFacebookRequest) {
    afterAuthenticate(ctx, logger, nk, data);
}

/**
 * Set up the user after first authentication.
 */
function afterAuthenticate(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: nkruntime.Session) {
    logger.info('after auth called, created: %s', data.create);
    if (!data.create) {
        // Account already exists.
        return
    }

    // For demo purposes set some dummy values for the user.
    const initialState = {
        'level': Math.floor(Math.random() * 100),
        'wins': Math.floor(Math.random() * 100),
        'gamesPlayed': Math.floor(Math.random() * 200),
    }

    const writeStats: nkruntime.StorageWriteRequest = {
        collection: 'stats',
        key: 'public',
        permissionRead: 2,
        permissionWrite: 0,
        value: initialState,
        userId: ctx.userId,
    }

    const writeAddFriendQuest = addFriendQuestInit(ctx.userId);

    // Set the default card collection for the new user.
    const writeCards: nkruntime.StorageWriteRequest = {
        collection: DeckCollectionName,
        key: DeckCollectionKey,
        permissionRead: DeckPermissionRead,
        permissionWrite: DeckPermissionWrite,
        value: defaultCardCollection(nk, logger, ctx.userId),
        userId: ctx.userId,
    }

    try {
        nk.storageWrite([writeStats, writeAddFriendQuest, writeCards]);
    } catch (error) {
        logger.error('storageWrite error: %q', error);
        throw error;
    }
}

/**
 * Find a user by a wildcard search on their username.
 */
const rpcSearchUsernameFn: nkruntime.RpcFunction =
        function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
    const input: any = JSON.parse(payload)

    // NOTE: Must be very careful with custom SQL queries to performance check them.
    const query = `
    SELECT id, username FROM users WHERE username ILIKE concat($1, '%')
    `
    const result = nk.sqlQuery(query, [input.username]);

    return JSON.stringify(result);
}
