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

const winnerBonus = 10;
const towerDestroyedMultiplier = 5;
const speedBonus = 5;
const winReward = 180;
const loseReward = 110;

function calculateScore(isWinner: boolean, towersDestroyed: number, matchDuration: number): number {
    let score = isWinner ? winnerBonus : 0;

    score += towersDestroyed * towerDestroyedMultiplier;

    let durationMin = Math.floor(matchDuration / 60);

    let timeScore = 0;
    if (isWinner) {
        timeScore = Math.max(1, speedBonus - durationMin);
    } else {
        timeScore = Math.max(1, Math.min(durationMin, speedBonus));
    }

    score += timeScore;

    //changeset values must be whole numbers
    return Math.round(score);
}

function rpcGetMatchScore(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
    let matchId = JSON.parse(payload)['match_id'];
    if (!matchId) {
        throw Error('missing match_id from payload');
    }

    let items = nk.walletLedgerList(ctx.userId, 100);
    while (items.cursor) {
        items = nk.walletLedgerList(ctx.userId, 100, items.cursor);
    }

    let lastMatchReward = {} as nkruntime.WalletLedgerResult;
    for (let update of items.items) {
        if (update.metadata.source === 'match_reward'
            && update.metadata.match_id === matchId) {
            lastMatchReward = update;
        }
    }

    return JSON.stringify(lastMatchReward);
}

enum MatchEndPlacement {
    Loser = 0,
    Winner = 1
}

interface MatchEndRequest {
    matchId : string;
    placement: MatchEndPlacement;
    time : number;
    towersDestroyed : number;
}

interface MatchEndResponse {
    gems : number;
    score : number;
}

const rpcHandleMatchEnd: nkruntime.RpcFunction = function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {

    if (!payload) {
        throw Error('no data found in rpc payload');
    }

    let request : MatchEndRequest = JSON.parse(payload);

    let score = calculateScore(request.placement == MatchEndPlacement.Winner, request.towersDestroyed, request.time);

    let metadata = {
        source: 'match_reward',
        match_id: request.matchId,
    };

    updateWallet(nk, ctx.userId, score, metadata);

    nk.leaderboardRecordWrite(globalLeaderboard, ctx.userId, ctx.username, score);

    let response : MatchEndResponse = {
        gems: request.placement == MatchEndPlacement.Winner ? winReward : loseReward,
        score: score
    };

    logger.debug('match %s ended', ctx.matchId);

    return JSON.stringify(response);
}
