using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace COE.Tests
{
    [TestClass]
    public class DataTests
    {
        [TestMethod]
        public void ValidateNames()
        {
            var names = Enum.GetValues(typeof(Name));

            Assert.AreEqual(names.Length, Data.Family.Count, "Unexpected number of elements");

            foreach (Name name in names)
            {
                Assert.IsTrue(Data.Family.Any(f => f.Name == name), "Unable to find Name: " + name);
            }
        }

        [TestMethod]
        public void ValidateSpouses()
        {
            foreach (var person in Data.Family)
            {
                if (person.Spouse != null)
                {
                    Assert.IsTrue(person.Name != person.Spouse, $"Spouce mismatch for: {person.Name}");
                    Assert.AreEqual(person.Name, Data.Family.Single(p => p.Name == person.Spouse).Spouse, $"Spouse mismatch for: {person.Name}");
                }
            }
        }

        [TestMethod]
        public void ValidateSiblings()
        {
            foreach (var person in Data.Family)
            {
                if (person.Siblings != null)
                {
                    foreach (var sibling in person.Siblings)
                    {
                        Assert.IsTrue(person.Name != sibling, $"Sibling mistmach for: {person.Name}");
                        Assert.IsTrue(sibling.GetPerson().Siblings.Contains(person.Name), $"Sibling mistmach for: {person.Name}");
                    }
                }
            }
        }

        [TestMethod]
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

                    Assert.IsTrue(recipients.Add(recipient), $"{recipient} has already been given to");
                    Assert.IsTrue(participant.Name != recipient, $"Gift mismatch for: {participant.Name} in: {year}");
                    Assert.IsTrue(participants.Contains(recipient.GetPerson()), $"Gift mismatch for: {participant.Name} in: {year}");
                }
            }
        }
    }
}
