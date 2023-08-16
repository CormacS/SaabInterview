using System;

namespace TicketManagementSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Ticket Service Test Harness");

            var service = new TicketService();

            var ticketId = service.CreateTicket(
                "System Crash",
                Priority.Medium,
                "Johan",
                "The system crashed when user performed a search",
                DateTime.UtcNow,
                true);

            service.AssignTicket(ticketId, "Michael");

            Console.WriteLine("Done");
        }
    }
}
