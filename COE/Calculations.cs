using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COE
{
    public static class Calculations
    {
        public static List<Pairing> GetPairings(this int[,] matrix, List<Response> participants)
        {
            List<Pairing> results = new List<Pairing>();

            int matchCount = 0;
            Name personToMatch = participants.First().Name;
            var personToMatchIndex = 0;
            HashSet<int> matchedIndexes = new HashSet<int>();
            matchedIndexes.Add(personToMatchIndex);

            while (matchCount < participants.Count - 1)
            {
                var potentialMatches = matrix.GetPotentialMatches()
                    .Where(pm => !Program.ForcedMatchees.Contains(participants[pm.Index].Name)) // Filter out anyone that we're fixing their match
                    .OrderByDescending(pm => pm.TotalWeight)
                    .ToList();

                var bestMatchIndex = -1;
                var bestMatchWeight = int.MaxValue;

                foreach (var potentialMatch in potentialMatches)
                {
                    var matchWeight = matrix[personToMatchIndex, potentialMatch.Index];

                    if (matchWeight < bestMatchWeight && !matchedIndexes.Contains(potentialMatch.Index))
                    {
                        bestMatchWeight = matchWeight;
                        bestMatchIndex = potentialMatch.Index;
                    }
                }

                // Fix a match if necessary
                if (Program.ForcedMatches.ContainsKey(personToMatch))
                {
                    var forcedMatch = Program.ForcedMatches[personToMatch];
                    bestMatchIndex = participants.IndexOf(participants.Single(p => p.Name == forcedMatch));
                    bestMatchWeight = -1;
                }

                matchedIndexes.Add(bestMatchIndex);
                results.Add(new Pairing { Giver = personToMatch, Receiver = participants[bestMatchIndex].Name, Weight = bestMatchWeight });

                personToMatch = participants[bestMatchIndex].Name;
                personToMatchIndex = bestMatchIndex;

                matchCount++;
            }

            results.Add(new Pairing { Giver = personToMatch, Receiver = participants.First().Name, Weight = matrix[personToMatchIndex, 0]});

            return results;
        }

        public static void PrintPairings(this List<Pairing> pairings)
        {
            int i = 0;
            foreach (var pairing in pairings)
            {
                Console.WriteLine("{0, 2} ({1}): {2} -> {3}", ++i, pairing.Weight, pairing.Giver, pairing.Receiver);
            }
        }

        private static List<Match> GetPotentialMatches(this int[,] matrix)
        {
            List<Match> results = new List<Match>();

            var matrixSize = matrix.GetUpperBound(0);

            for (int i = 0; i <= matrixSize; i++)
            {
                var totalWeight = 0;

                for (int j = 0; j < matrixSize; j++)
                {
                    totalWeight += matrix[i, j];
                }

                results.Add(new Match { Index = i, TotalWeight = totalWeight });
            }

            return results;
        }

        private class Match
        {
            public int Index { get; set; }

            public int TotalWeight { get; set; }
        }
    }
}
