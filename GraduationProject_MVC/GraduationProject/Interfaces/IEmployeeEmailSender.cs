namespace GraduationProject.Interfaces
{
    public interface IEmployeeEmailSender
    {
        Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
    }
}
