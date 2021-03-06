﻿using System;
using System.Collections.Generic;
using System.IO;

namespace COE
{
    public static class Parsing
    {
        public static List<Response> GetResponses(string filePath)
        {
            var names = new HashSet<Name>();
            var results = new List<Response>();

            if (!File.Exists(filePath))
            {
                throw new ArgumentException("File does not exist", nameof(filePath));
            }

            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var split = line.Split(";".ToCharArray());

                var name = GetNameFromString(split[0]);

                if (!names.Add(name))
                {
                    throw new ArgumentException($"Duplicate name: {name}");
                }

                results.Add(new Response
                {
                    Name = name,
                    IsParticipating = string.Equals(split[1], "Yes", StringComparison.InvariantCultureIgnoreCase),
                    Address = split.Length == 3 ? split[2] : null
                });
            }

            return results;
        }

        public static int[,] GetCompatibilityMatrix(List<Response> participants)
        {
            var result = new int[participants.Count, participants.Count];

            for (int i = 0; i < participants.Count; i++)
            {
                var person = participants[i].Name.GetPerson();

                // Process spouse
                if (person.Spouse != null)
                {
                    var index = participants.FindIndex(p => p.Name == person.Spouse);

                    if (index != -1)
                    {
                        result[i, index] += 50;
                    }
                }

                // Process siblings
                if (person.Siblings != null)
                {
                    foreach (var sibling in person.Siblings)
                    {
                        var index = participants.FindIndex(p => p.Name == sibling);

                        if (index != -1)
                        {
                            result[i, index] += 25;
                        }
                    }
                }

                // Process history
                foreach (var yearInHistory in person.History)
                {
                    if (yearInHistory.Value != null)
                    {
                        var index = participants.FindIndex(p => p.Name == yearInHistory.Value);

                        if (index != -1)
                        {
                            result[i, index] += yearInHistory.Key - 2001; // The first year of data is from 2002
                        }
                    }
                }
            }

            return result;
        }

        public static void PrintMatrix(this int[,] matrix, List<Response> participants)
        {
            Console.Write("   ");

            foreach (Response participant in participants)
            {
                Console.Write($"{participant.Name.GetInitials(),3}");
            }

            Console.WriteLine();

            for (int i = 0; i < participants.Count; i++)
            {
                Console.Write($"{participants[i].Name.GetInitials(),3}");

                for (int j = 0; j < participants.Count; j++)
                {
                    Console.Write($"{matrix[i, j],3}");
                }

                Console.WriteLine();
            }
        }

        private static string GetInitials(this Name name)
        {
            var value = name.ToString().Split("_".ToCharArray());

            return value[0].Substring(0, 1) + value[1].Substring(0, 1);
        }

        private static Name GetNameFromString(string name)
        {
            name = name
                .Replace("\'", "")
                .Replace(" ", "_");

            return (Name)Enum.Parse(typeof(Name), name);
        }
    }
}
