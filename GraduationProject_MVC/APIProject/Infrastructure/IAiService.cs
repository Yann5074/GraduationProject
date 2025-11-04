namespace ApiProject.Infrastructure
{
    public interface IAiService
    {
        Task<string> CompleteAsync(string userText, string? clientSummary = null);
    }
}
