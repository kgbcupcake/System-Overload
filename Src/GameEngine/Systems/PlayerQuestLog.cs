using System.Collections.Generic;

namespace System_Overload.Src.GameEngine.Systems
{
    public class PlayerQuestLog
    {
        // Singleton or static instance for session persistence
        private static PlayerQuestLog? _instance;
        public static PlayerQuestLog Instance => _instance ??= new PlayerQuestLog();

        public List<string> AcceptedQuestIds { get; private set; } = new List<string>();
        public List<string> CompletedQuestIds { get; private set; } = new List<string>();

        public void AcceptQuest(string questId)
        {
            if (!AcceptedQuestIds.Contains(questId) && !CompletedQuestIds.Contains(questId))
            {
                AcceptedQuestIds.Add(questId);
            }
        }

        public void CompleteQuest(string questId)
        {
            if (AcceptedQuestIds.Contains(questId))
            {
                AcceptedQuestIds.Remove(questId);
            }

            if (!CompletedQuestIds.Contains(questId))
            {
                CompletedQuestIds.Add(questId);
            }
        }

        public bool IsQuestAccepted(string questId) => AcceptedQuestIds.Contains(questId);
        public bool IsQuestCompleted(string questId) => CompletedQuestIds.Contains(questId);
        public bool IsQuestAvailable(string questId) => !IsQuestAccepted(questId) && !IsQuestCompleted(questId);
    }
}
