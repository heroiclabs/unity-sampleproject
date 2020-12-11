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

enum ClanNotificationCode {
    Refresh = 2,
    Delete = 3
}

/**
 * Send an in-app notification to all clan members when a new member joins.
 */
const afterJoinGroupFn: nkruntime.AfterHookFunction<void, nkruntime.JoinGroupRequest> =
        function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: void, request: nkruntime.JoinGroupRequest) {
    sendGroupNotification(nk, request.groupId ?? "", ClanNotificationCode.Refresh, "New Member Joined!");
}

/**
 * Send an in-app notification to all clan members when one or more members are kicked.
 */
const afterKickGroupUsersFn: nkruntime.AfterHookFunction<void, nkruntime.KickGroupUsersRequest> =
        function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: void, request: nkruntime.KickGroupUsersRequest) {
    sendGroupNotification(nk, request.groupId ?? "", ClanNotificationCode.Refresh, "Member(s) Have Been Kicked!");
}

/**
 * Send an in-app notification to all clan members when a member leaves.
 */
const afterLeaveGroupFn: nkruntime.AfterHookFunction<void, nkruntime.LeaveGroupRequest> =
        function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: void, request: nkruntime.LeaveGroupRequest) {
    sendGroupNotification(nk, request.groupId ?? "", ClanNotificationCode.Refresh, "Member Left!");
}

/**
 * Send an in-app notification to all clan members when one or more members are promoted.
 */
const afterPromoteGroupUsersFn: nkruntime.AfterHookFunction<void, nkruntime.PromoteGroupUsersRequest> =
        function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: void, request: nkruntime.PromoteGroupUsersRequest) {
    sendGroupNotification(nk, request.groupId ?? "", ClanNotificationCode.Refresh, "Member(s) Have Been Promoted!");
}

/**
 * Send an in-app notification to the clan members when the superadmin deletes it.
 */
const beforeDeleteGroupFn: nkruntime.BeforeHookFunction<nkruntime.DeleteGroupRequest> =
        function(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, request: nkruntime.DeleteGroupRequest): nkruntime.DeleteGroupRequest {
    const members = nk.groupUsersList(request.groupId!, 100, 0);

    // Check delete request user is a superadmin in the group.
    members.groupUsers?.every(user => {
        if (user.user.userId == ctx.userId) {
            sendGroupNotification(nk, request.groupId ?? "", ClanNotificationCode.Delete, "Clan Deleted!");
            return false;
        }
        return true;
    });

    return request
}

function sendGroupNotification(nk: nkruntime.Nakama, groupId: string, code: ClanNotificationCode, subject: string) {
    const members = nk.groupUsersList(groupId, 100);
    const count = (members.groupUsers ?? []).length;
    if (count < 1) {
        return;
    }

    const notifications: nkruntime.NotificationRequest[] = new Array(count);
    members.groupUsers?.forEach(user => {
        const n: nkruntime.NotificationRequest = {
            code: code,
            content: {},
            persistent: false,
            subject: subject,
            userId: user.user.userId,
        }
        notifications.push(n);
    });

    nk.notificationsSend(notifications);
}
