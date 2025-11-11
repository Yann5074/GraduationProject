using ApiProject.DTOs.Responses;

namespace ApiProject.Services
{
    public interface IAiProductService
    {
    Task<ResCompleteResult> ParseAndQueryAsync(string message, CancellationToken ct = default);
    }
}