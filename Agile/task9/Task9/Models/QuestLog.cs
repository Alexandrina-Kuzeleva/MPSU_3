using System;
using System.Collections.Generic;

namespace Task9.Models
{
    public class QuestLog
    {
        private readonly List<Quest> _quests = new();
        private readonly Dictionary<string, Quest> _byId = new();

        public int Count => _quests.Count;

        public Quest this[int index]
        {
            get
            {
                if (index < 0 || index >= _quests.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _quests[index];
            }
        }

        public Quest this[string id]
        {
            get
            {
                if (id == null)
                    throw new ArgumentNullException(nameof(id));
                if (!_byId.TryGetValue(id, out var quest))
                    throw new KeyNotFoundException($"Quest with id '{id}' not found.");
                return quest;
            }
        }

        public void Add(Quest quest)
        {
            if (quest == null)
                throw new ArgumentNullException(nameof(quest));
            if (_byId.ContainsKey(quest.Id))
                throw new ArgumentException($"Quest with id '{quest.Id}' already exists.", nameof(quest));

            _quests.Add(quest);
            _byId[quest.Id] = quest;
        }

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= _quests.Count)
                return false;

            var quest = _quests[index];
            _quests.RemoveAt(index);
            _byId.Remove(quest.Id);
            return true;
        }

        public bool RemoveById(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (!_byId.TryGetValue(id, out var quest))
                return false;

            _byId.Remove(id);
            _quests.Remove(quest);
            return true;
        }

        public IEnumerable<Quest> EnumerateByDifficulty(Difficulty minDifficulty)
        {
            foreach (var quest in _quests)
            {
                if (quest.Difficulty >= minDifficulty)
                    yield return quest;
            }
        }

        public IEnumerator<Quest> GetEnumerator() => _quests.GetEnumerator();
    }
}
