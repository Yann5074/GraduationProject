using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ApiProject.DTOs
{
    // 用來接 multipart/form-data 的表單
    public class UploadPhotoForm
    {
        // 這個 Name 一定要叫 "file"（等同你在 Swagger/前端表單欄位的名字）
        [FromForm(Name = "file")]
        public IFormFile File { get; set; }
    }
}
