using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatCafe.Social
{
    [Serializable]
    public class FriendProfile
    {
        public string userId;
        public string friendCode;
        public string playerName;
        public int cafeLevel;
        public int catCount;
        public float totalMoney;
        public bool isOnline;
        public long lastActiveTimestamp;

        public string DisplayStatus => isOnline ? "🟢 Đang mở quán" : "⚪ Vắng mặt";
    }

    [Serializable]
    public class ChatMessage
    {
        public string messageId;
        public string senderId;
        public string senderName;
        public string content;
        public string channel; // "global" hoặc "friend_uid"
        public long timestamp;

        public string FormattedTime
        {
            get
            {
                try
                {
                    DateTime dt = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime().DateTime;
                    return dt.ToString("HH:mm");
                }
                catch
                {
                    return "";
                }
            }
        }
    }

    [Serializable]
    public class GiftItem
    {
        public string giftId;
        public string senderId;
        public string senderName;
        public string giftType; // "money", "gems", "treat", "beans"
        public int amount;
        public string message;
        public bool isClaimed;
        public long timestamp;
    }

    [Serializable]
    public class VisitorInteraction
    {
        public string visitorId;
        public string visitorName;
        public string targetCatName;
        public string actionType; // "pet", "feed"
        public long timestamp;
    }

    [Serializable]
    public class CoOpQuest
    {
        public string questId;
        public string title;
        public string description;
        public int targetCount;
        public int currentCount;
        public int rewardGems;
        public float rewardMoney;
        public bool isCompleted;
        public bool isClaimed;
    }
}
