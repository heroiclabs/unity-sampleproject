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

const DeckPermissionRead = 2;
const DeckPermissionWrite = 0;
const DeckCollectionName = 'card_collection';
const DeckCollectionKey = 'user_cards';

const DefaultDeckCards = [
    {
        type: 1,
        level: 1,
    },
    {
        type: 1,
        level: 1,
    },
    {
        type: 2,
        level: 1,
    },
    {
        type: 2,
        level: 1,
    },
    {
        type: 3,
        level: 1,
    },
    {
        type: 4,
        level: 1,
    },
];

const DefaultStoredCards = [
    {
        type: 2,
        level: 1,
    },
    {
        type: 2,
        level: 1,
    },
    {
        type: 3,
        level: 1,
    },
    {
        type: 4,
        level: 1,
    },
];

type CardType = 1 | 2 | 3 | 4;
type CardMap = {[id: string]: Card}

interface Card {
    type: CardType
    level: number
}

interface CardCollection {
    deckCards: CardMap
    storedCards: CardMap
}

interface SwapDeckCardRequest {
    cardInId: string
    cardOutId: string
}

interface UpgradeCardRequest {
    id: string
}

/**
 * Swap a card in the user deck with one in its collection.
 */
const rpcSwapDeckCard: nkruntime.RpcFunction =
        function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
    const request: SwapDeckCardRequest = JSON.parse(payload);

    const userCards = loadUserCards(nk, logger, ctx.userId);

    // Check the cards being swapper are valid.
    if (Object.keys(userCards.deckCards).indexOf(request.cardOutId) < 0) {
        throw Error('invalid out card');
    }
    if (Object.keys(userCards.storedCards).indexOf(request.cardInId) < 0) {
        throw Error('invalid in card');
    }

    // Swap the cards
    let outCard = userCards.deckCards[request.cardOutId];
    let inCard = userCards.storedCards[request.cardInId];
    delete(userCards.deckCards[request.cardOutId]);
    delete(userCards.storedCards[request.cardInId]);
    userCards.deckCards[request.cardInId] = inCard;
    userCards.storedCards[request.cardOutId] = outCard;

    // Store the changes
    storeUserCards(nk, logger, ctx.userId, userCards);

    logger.debug("user '%s' deck card '%s' swapped with '%s'", ctx.userId);

    return JSON.stringify(userCards);
}


/**
 * Upgrade the level of a given card in the user collection.
 */
const rpcUpgradeCard: nkruntime.RpcFunction =
        function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
    let request: UpgradeCardRequest = JSON.parse(payload);

    let userCards = loadUserCards(nk, logger, ctx.userId);
    if (!userCards) {
        logger.error('user %s card collection not found', ctx.userId);
        throw Error('Internal server error');
    }

    let card = userCards.deckCards[request.id];
    if (card) {
        card.level += 1;
        userCards.deckCards[request.id] = card;
    }

    card = userCards.storedCards[request.id];
    if (card) {
        card.level += 1;
        userCards.storedCards[request.id] = card;
    }

    if (!card) {
        logger.error('invalid card');
        throw Error('invalid card');
    }

    try {
        storeUserCards(nk, logger, ctx.userId, userCards);
    } catch(error) {
        // Error logged in storeUserCards
        throw Error('Internal server error');
    }

    logger.debug('user %s card %s upgraded', ctx.userId, JSON.stringify(card));

    return JSON.stringify(card);
}

/**
 * Reset user card collection to the default set.
 */
const rpcResetCardCollection: nkruntime.RpcFunction =
        function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string) {
    let collection = defaultCardCollection(nk, logger, ctx.userId);
    storeUserCards(nk, logger, ctx.userId, collection);

    logger.debug('user %s card collection has been reset', ctx.userId);
    return JSON.stringify(collection);
}

/**
 * Get user card collection.
 */
const rpcLoadUserCards: nkruntime.RpcFunction =
        function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string) : string {
    return JSON.stringify(loadUserCards(nk, logger, ctx.userId));
}

/**
 * Add a random card to the user collection for 100 gems.
 */
const rpcBuyRandomCard: nkruntime.RpcFunction =
        function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string) {
    let type = Math.floor(Math.random() * 4) + 1 as CardType;

    let userCards: CardCollection;
    try {
        userCards = loadUserCards(nk, logger, ctx.userId);
    } catch (error) {
        logger.error('error loading user cards: %s', error.message);
        throw Error('Internal server error');
    }

    let cardId = nk.uuidv4();
    let newCard: Card = {
        type,
        level: 1,
    }

    userCards.storedCards[cardId] = newCard;

    try {
        // If no sufficient funds are available, this will throw an error.
        nk.walletUpdate(ctx.userId, {[currencyKeyName]: -100});
        // Store the new card to the collection.
        storeUserCards(nk, logger, ctx.userId, userCards);
    } catch(error) {
        logger.error('error buying card: %s', error.message);
        throw error;
    }

    logger.debug('user %s successfully bought a new card', ctx.userId);

    return JSON.stringify({[cardId]: newCard});
}

function loadUserCards(nk: nkruntime.Nakama, logger: nkruntime.Logger, userId: string): CardCollection {
    let storageReadReq: nkruntime.StorageReadRequest = {
        key: DeckCollectionKey,
        collection: DeckCollectionName,
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
        throw Error('user cards storage object not found');
    }

    let storedCardCollection = objects[0].value as CardCollection;
    return storedCardCollection;
}

function storeUserCards(nk: nkruntime.Nakama, logger: nkruntime.Logger, userId: string, cards: CardCollection) {
    try {
        nk.storageWrite([
            {
                key: DeckCollectionKey,
                collection: DeckCollectionName,
                userId: userId,
                value: cards,
                permissionRead: DeckPermissionRead,
                permissionWrite: DeckPermissionWrite,
            }
        ]);
    } catch(error) {
        logger.error('storageWrite error: %s', error.message);
        throw error;
    }
}

function getRandomInt(min : number, max: number) {
    return min + Math.floor(Math.random() * Math.floor(max));
}

function defaultCardCollection(nk: nkruntime.Nakama, logger: nkruntime.Logger, userId: string): CardCollection {
    let deck: any = {};
    DefaultDeckCards.forEach(c => {
        deck[nk.uuidv4()] = c;
    });

    let stored: any = {};
    DefaultStoredCards.forEach(c => {
        stored[nk.uuidv4()] = c;
    })

    let cards: CardCollection = {
        deckCards: deck,
        storedCards: stored,
    }

    storeUserCards(nk, logger, userId, cards);

    return {
        deckCards: deck,
        storedCards: stored,
    }
}
