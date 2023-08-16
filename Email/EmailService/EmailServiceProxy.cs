using System;
namespace EmailService
{
    public class EmailServiceProxy : IEmailService
    {
        public void SendEmailToAdministrator(string incidentTitle, string assignedTo)
        {
            if (incidentTitle == null)
            {
                throw new ArgumentNullException(nameof(incidentTitle));
            }

            // Some internal logic to send email.
            // Assume this is implemented and does not need to be changed.
        }
    }
}
