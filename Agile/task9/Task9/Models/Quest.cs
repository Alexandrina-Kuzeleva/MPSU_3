using System;
using System.Collections.Generic;

namespace Task9.Models
{
    public class Quest
    {
        private readonly List<Objective> _objectives = new();

        public string Id { get; }
        public string Title { get; }
        public Difficulty Difficulty { get; }
        public IReadOnlyList<Objective> Objectives => _objectives;

        public Quest(string id, string title, Difficulty difficulty)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Quest ID cannot be null or whitespace.", nameof(id));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Quest title cannot be null or whitespace.", nameof(title));

            Id = id;
            Title = title;
            Difficulty = difficulty;
        }

        public void AddObjective(Objective objective)
        {
            if (objective == null)
                throw new ArgumentNullException(nameof(objective));

            _objectives.Add(objective);
        }

        public override string ToString() => $"{Title} [{Difficulty}] (Objectives: {Objectives.Count})";
    }
}
