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

        private bool ShouldRaisePriority(string title, DateTime date)
        {
            string[] keywords = { "Crash", "Important", "Failure" };
            bool containsKeyword = keywords.Any(keyword => title.Contains(keyword));

            if (containsKeyword || date < DateTime.UtcNow - TimeSpan.FromHours(1))
            {
                return true;
            }

            return false;
        }

        private Priority RaisePriority(Priority priority)
        {
            if (priority == Priority.Low)
            {
                priority = Priority.Medium;
            }
            else if (priority == Priority.Medium)
            {
                priority = Priority.High;
            }

            return priority;
        }
        private void GetPrice(Priority priority, bool isPayingCustomer, out double price, out User accountManager)
        {
            price = 0;
            accountManager = null;
            if (isPayingCustomer)
            {
                // Only paid customers have an account manager.
                using (var ur = new UserRepository())
                {
                    accountManager = ur.GetAccountManager();
                }

                if (priority == Priority.High)
                {
                    price = 100;
                }
                else
                {
                    price = 50;
                }
            }
        }

        public int CreateTicket(string title, Priority priority, string assignedTo, string desc, DateTime date, bool isPayingCustomer)
        {
            // Check if title or desc are null or if they are invalid and throw exception
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(desc))
            {
                throw new InvalidTicketException("Title or description were null");
            }

            User user = GetUser(assignedTo);

            bool priorityRaised = ShouldRaisePriority(title, date);

            if (priorityRaised)
            {
                priority = RaisePriority(priority);
            }

            if (priority == Priority.High)
            {
                var emailService = new EmailServiceProxy();
                emailService.SendEmailToAdministrator(title, assignedTo);
            }

            double price;
            User accountManager;

            GetPrice(priority, isPayingCustomer, out price, out accountManager);

            var ticket = new Ticket()
            {
                Title = title,
                AssignedUser = user,
                Priority = priority,
                Description = desc,
                Created = date,
                PriceDollars = price,
                AccountManager = accountManager
            };

            var id = TicketRepository.CreateTicket(ticket);

            // Return the id
            return id;
        }

        public void AssignTicket(int id, string username)
        {
            User user = GetUser(username);

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
