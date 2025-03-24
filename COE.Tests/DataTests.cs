using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace COE.Tests;

[TestFixture]
public class DataTests
{
    [Test]
    public void ValidateNames()
    {
        var names = Enum.GetValues(typeof(Name));

        Assert.That(names.Length, Is.EqualTo(Data.Family.Count), "Unexpected number of elements");

        foreach (Name name in names)
        {
            Assert.That(Data.Family.Any(f => f.Name == name), Is.True, "Unable to find Name: " + name);
        }
    }

    [Test]
    public void ValidateSpouses()
    {
        foreach (var person in Data.Family)
        {
            if (person.Spouse != null)
            {
                Assert.That(person.Name != person.Spouse, Is.True, $"Spouce mismatch for: {person.Name}");
                Assert.That(person.Name, Is.EqualTo(Data.Family.Single(p => p.Name == person.Spouse).Spouse), $"Spouse mismatch for: {person.Name}");
            }
        }
    }

    [Test]
    public void ValidateSiblings()
    {
        foreach (var person in Data.Family)
        {
            if (person.Siblings != null)
            {
                foreach (var sibling in person.Siblings)
                {
                    Assert.That(person.Name != sibling, Is.True, $"Sibling mistmach for: {person.Name}");
                    Assert.That(sibling.GetPerson().Siblings.Contains(person.Name), Is.True, $"Sibling mistmach for: {person.Name}");
                }
            }
        }
    }

    [Test]
    public void ValidateFullHistory()
    {
        var years = Name.Rob_Keim.GetPerson().History.Keys;

        foreach (var person in Data.Family)
        {
            foreach (var year in years)
            {
                Assert.That(person.History.ContainsKey(year), Is.True, $"Missing history for {person.Name} in year {year}");
            }
        }
    }

    [Test]
    public void ValidateYears()
    {
        var years = Name.Rob_Keim.GetPerson().History.Keys;

        foreach (var year in years)
        {
            var participants = Data.Family.Where(p => p.History[year] != null).ToList();
            var recipients = new HashSet<Name>();

            foreach (var participant in participants)
            {
                var recipient = participant.History[year].Value;

                Assert.That(recipients.Add(recipient), Is.True, $"{recipient} has already been given to");
                Assert.That(participant.Name != recipient, Is.True, $"Gift mismatch for: {participant.Name} in: {year}");
                Assert.That(participants.Contains(recipient.GetPerson()), Is.True, $"Gift mismatch for: {participant.Name} in: {year}");
            }
        }
    }
}
