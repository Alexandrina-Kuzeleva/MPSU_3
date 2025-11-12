using System;

namespace Task9.Models
{
    public class Objective
    {
        public string Code { get; }
        public string Description { get; }
        public int RequiredCount { get; }

        public Objective(string code, string description, int requiredCount = 1)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException(
                    "Objective code cannot be null or whitespace.", nameof(code)
                    );

            if (requiredCount < 1)
                throw new ArgumentOutOfRangeException(nameof(requiredCount), "Required count must be at least 1.");

            Code = code;
            Description = description ?? string.Empty;
            RequiredCount = requiredCount;
        }

        public override string ToString() => $"{Code} ({RequiredCount}) - {Description}";
    }
}
