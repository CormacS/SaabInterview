using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.Json;
using EmailService;

namespace TicketManagementSystem
{
    public class TicketService
    {
        private User GetUser(string username)
        {
            User user = null;

            using (var ur = new UserRepository())
            {
                  user = ur.GetUser(username);
            }

            if (user == null)
            {
                throw new UnknownUserException("User " + username + " not found");
            }

            return user;
        }

        public int CreateTicket(string t, Priority p, string assignedTo, string desc, DateTime d, bool isPayingCustomer)
        {
            // Check if t or desc are null or if they are invalid and throw exception
            if (string.IsNullOrEmpty(t) || string.IsNullOrEmpty(desc))
            {
                throw new InvalidTicketException("Title or description were null");
            }

            User user = GetUser(assignedTo);

            bool priorityRaised = ShouldRaisePriority(t, d);

            if (priorityRaised)
            {
                p = RaisePriority(p);
            }


            if (p == Priority.High)
            {
                var emailService = new EmailServiceProxy();
                emailService.SendEmailToAdministrator(t, assignedTo);
            }

            double price = 0;
            User accountManager = null;
            if (isPayingCustomer)
            {
                // Only paid customers have an account manager.
                using (var ur = new UserRepository())
                {
                    accountManager = ur.GetAccountManager();

                }

                if (p == Priority.High)
                {
                    price = 100;
                }
                else
                {
                    price = 50;
                }
            }

            var ticket = new Ticket()
            {
                Title = t,
                AssignedUser = user,
                Priority = p,
                Description = desc,
                Created = d,
                PriceDollars = price,
                AccountManager = accountManager
            };

            var id = TicketRepository.CreateTicket(ticket);

            // Return the id
            return id;
        }

        private static bool ShouldRaisePriority(string t, DateTime d)
        {
            string[] keywords = { "Crash", "Important", "Failure" };
            bool containsKeyword = keywords.Any(keyword => t.Contains(keyword));

            if (containsKeyword || d < DateTime.UtcNow - TimeSpan.FromHours(1))
            {
                return true;
            }

            return false;
        }

        private static Priority RaisePriority(Priority p)
        {
            if (p == Priority.Low)
            {
                p = Priority.Medium;
            }
            else if (p == Priority.Medium)
            {
                p = Priority.High;
            }

            return p;
        }

        public void AssignTicket(int id, string username)
        {
            User user = null;
            using (var ur = new UserRepository())
            {
                if (username != null)
                {
                    user = ur.GetUser(username);
                }
            }

            if (user == null)
            {
                throw new UnknownUserException("User not found");
            }

            var ticket = TicketRepository.GetTicket(id);

            if (ticket == null)
            {
                throw new ApplicationException("No ticket found for id " + id);
            }

            ticket.AssignedUser = user;

            TicketRepository.UpdateTicket(ticket);
        }
    }

    public enum Priority
    {
        High,
        Medium,
        Low
    }
}
