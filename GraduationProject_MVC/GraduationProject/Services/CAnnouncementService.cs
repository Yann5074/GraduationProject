using GraduationProject.DTOs;
using GraduationProject.Interfaces;

namespace GraduationProject.Services
{
    public class CAnnouncementService : IAnnouncementService
    {
        private readonly HttpClient _api;
        public CAnnouncementService(IHttpClientFactory factory)
        {
            _api = factory.CreateClient("Api");
        }

        public Task<List<CAnnouncementDTO>?> GetAllAsync(bool? active = null)
        {
            return _api.GetFromJsonAsync<List<CAnnouncementDTO>>(
                active.HasValue
                ? $"api/announcement?active={active.Value}"
                : "api/announcement");

            // 暫時不呼叫 API，直接回傳假資料
            //var mock = new List<CAnnouncementDTO>
            //{
            //    new CAnnouncementDTO
            //    {
            //        Id = 1,
            //        Title = "測試公告",
            //        Message = "API 尚未完成，這是假資料",
            //        StartAt = DateTime.UtcNow.AddDays(-1),
            //        EndAt = DateTime.UtcNow.AddDays(7),
            //        IsActive = true,
            //        Priority = 1,
            //        LastUpdated = DateTime.UtcNow
            //    }
            //};
            //    return Task.FromResult<List<CAnnouncementDTO>?>(mock);
        }

        public Task<CAnnouncementDTO?> GetAsync(int id)
        {
            return _api.GetFromJsonAsync<CAnnouncementDTO>($"api/announcement/{id}");
        }

        public async Task CreateAsync(CSaveAnnouncementDTO dto)
        {
            var res = await _api.PostAsJsonAsync("api/announcement", dto);
            res.EnsureSuccessStatusCode();
        }

        public async Task UpdateAsync(int id, CSaveAnnouncementDTO dto)
        {
            var res = await _api.PutAsJsonAsync($"api/announcement/{id}", dto);
            res.EnsureSuccessStatusCode();
        }

        public Task DeleteAsync(int id)
        {
            return _api.DeleteAsync($"api/announcement/{id}");
        }
    }
}
