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

const currencyKeyName = 'gems'

const rpcAddUserGems: nkruntime.RpcFunction = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama): string {
    let walletUpdateResult = updateWallet(nk, ctx.userId, 100, {});
    let updateString = JSON.stringify(walletUpdateResult);

    logger.debug('Added 100 gems to user %s wallet: %s', ctx.userId, updateString);

    return updateString;
}

function updateWallet(nk: nkruntime.Nakama, userId: string, amount: number, metadata: {[key: string]: any}): nkruntime.WalletUpdateResult {
    const changeset = {
        [currencyKeyName]: amount,
    }
    let result = nk.walletUpdate(userId, changeset, metadata, true);

    return result;
}
