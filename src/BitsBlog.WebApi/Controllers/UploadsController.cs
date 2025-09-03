using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace BitsBlog.WebApi.Controllers
{
    [ApiController]
    public class UploadsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        public UploadsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("api/uploads")]
        [RequestSizeLimit(10_000_000)] // ~10MB
        public async Task<ActionResult<object>> Upload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("파일이 없습니다.");

            var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot"), "uploads");
            if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

            var ext = Path.GetExtension(file.FileName);
            var name = $"{Guid.NewGuid():N}{ext}";
            var savePath = Path.Combine(uploadsDir, name);
            using (var stream = System.IO.File.Create(savePath))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/{name}";
            return Ok(new { url });
        }
    }
}


