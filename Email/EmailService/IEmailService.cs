namespace EmailService
{
    public interface IEmailService
    {
        void SendEmailToAdministrator(string incidentTitle, string assignedTo);
    }
}