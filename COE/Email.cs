﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace COE
{
    public static class Email
    {
        private static readonly object lockObj = new object();

        public static void SendInitialEmail()
        {
            SendToAll("Who's in?", $@"Hello everyone,<br /><br />I hope that everyone is doing well, and am looking forward to catching up with many of you at Christmas time!<br /><br />Please click <a href=""{ConfigurationManager.AppSettings["GoogleFormUrl"]}""><b>here</b></a> to fill out the form and submit your response.  It only takes one minute to do!</b>");
        }

        public static void SendWallOfFame()
        {
            var responses = Parsing.GetResponses(Program.ResponsesDocument);
            var responsesString = string.Join("<br />", responses.Select(r => r.Name.GetFullName()).Where(r => r != "Rob Keim"));

            string body = $@"Thanks for everyone that has responded so far!  The next update will be the wall of shame so be sure to get your responses sent in ASAP to avoid being on that list!<br /><br />Here's the wall of fame for the people who have already responded (in order of their responses):<br /><br />{responsesString}<br /><br />For those who haven't responded yet, click <a href=""{ConfigurationManager.AppSettings["GoogleFormUrl"]}""><b>here</b></a> to do so.";

            SendToAll("Wall of fame", body);
        }

        public static void SendWallOfShame()
        {
            var responses = Parsing.GetResponses(Program.ResponsesDocument);
            var nonResponders = Data.Family.Where(p => !p.IsInactive && p.Name != Name.Rob_Keim).Select(p => p.Name).Except(responses.Select(r => r.Name)).OrderBy(p => p);
            var nonRespondersString = string.Join("<br />", nonResponders.Select(nr => nr.GetFullName()));

            string body = $@"As promised the wall of shame :)<br /><br />The following people have still NOT responded:<br /><br />{nonRespondersString}<br /><br />Please repsond <a href=""{ConfigurationManager.AppSettings["GoogleFormUrl"]}""><b>here</b></a> ASAP!";

            SendToAll("Wall of shame", body);
        }

        const string pairingMessageFormat = "Hello {0},<br /><br />You have <b>{1}</b> this year and you can send your ornament to:<br />{2}";

        public static void SendPairings()
        {
            var responses = Parsing.GetResponses(Program.ResponsesDocument);
            var participants = responses.Where(r => r.IsParticipating).ToList();

            var seed = 0;
            var matrix = Parsing.GetCompatibilityMatrix(participants);
            var pairings = matrix.GetPairings(participants);

            while (pairings.Sum(p => p.Weight) != 0)
            {
                Console.WriteLine($"Retry #{++seed}");
                var random = new Random(seed);
                participants = participants.OrderBy(p => random.Next()).ToList();
                matrix = Parsing.GetCompatibilityMatrix(participants);
                pairings = matrix.GetPairings(participants);
            }

            pairings.PrintPairings();

            Console.Write("Send pairings (y/n)?");
            var response = Console.ReadLine();

            if (response != null && response.StartsWith("y", StringComparison.InvariantCultureIgnoreCase))
            {
                // Sending these e-mails one by one as there were SMTP errors when I tried to send too many emails simultaneously
                foreach (var pairing in pairings)
                {
                    var giver = pairing.Giver.GetPerson();
                    var receiver = pairing.Receiver.GetPerson();
                    var address = responses.Single(r => r.Name == receiver.Name).Address;

                    if (address.Equals("Phillips", StringComparison.InvariantCultureIgnoreCase))
                    {
                        address = ConfigurationManager.AppSettings["PhillipsAddress"];
                    }
                    else if (address.Equals("Smith", StringComparison.InvariantCultureIgnoreCase))
                    {
                        address = ConfigurationManager.AppSettings["SmithAddress"];
                    }
                    else
                    {
                        // No need to modify the address
                    }

                    var message = string.Format(pairingMessageFormat, giver.Name.GetFirstName(), receiver.Name.GetFullName(), address);

                    SendEmailAsync(giver.Email, "Pairing", message).Wait();
                }

                SendToAll("Pairings sent", "The pairing have been sent!!  Let me know ASAP if there are any problems, and let the shopping begin!");
            }
            else
            {
                Console.WriteLine("Skipping sending pairings");
            }
        }

        private static void SendToAll(string subject, string body)
        {
            SendEmailAsync(Data.Family.Where(p => !p.IsInactive && p.Email != null).Select(p => p.Email), subject, body).Wait();
        }

        private static Task SendEmailAsync(string email, string subject, string body)
        {
            return SendEmailAsync(new[] { email }, subject, body);
        }

        private static Task SendEmailAsync(IEnumerable<string> rawTo, string subject, string body)
        {
            var to = rawTo.OrderBy(email => email).Distinct().ToList();

            subject = $"[CoE {Program.CurrentYear}] {subject}";
            body = $"{body}<br /><br />Love,<br />Rob";

            lock (lockObj)
            {
                Console.WriteLine("--------------------------------------------------------------");
                Console.WriteLine("To: {0}", string.Join(",", to));
                Console.WriteLine("Subject: {0}", subject);
                Console.WriteLine("Body:");
                Console.WriteLine(body.Replace("<br />", "\n").Replace("<b>", "").Replace("</b>", "").Replace("&nbsp;", ""));
                Console.WriteLine("--------------------------------------------------------------");
            }

            if (Program.EmailStatus != EmailStatus.Disabled)
            {
                if (Program.EmailStatus == EmailStatus.OnlyToMe)
                {
                    to = new List<string> { "robkeim@gmail.com" };
                }

                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("robkeim@gmail.com", ConfigurationManager.AppSettings["GmailPassword"]),
                    EnableSsl = true
                };

                var message = new MailMessage
                {
                    From = new MailAddress("robkeim@gmail.com"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                foreach (var email in to)
                {
                    message.To.Add(new MailAddress(email));
                }

                return client.SendMailAsync(message);
            }
            else
            {
                Console.WriteLine("Email sending disabled");
            }

            return Task.CompletedTask;
        }

        private static string GetFullName(this Name name)
        {
            return name.ToString().Replace("_", " ").Replace("OConnor", "O'Connor");
        }

        private static string GetFirstName(this Name name)
        {
            return name.ToString().Split("_".ToCharArray()).First();
        }
    }
}
