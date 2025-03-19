using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadFileController : ControllerBase
    {
        private readonly ILogger<UploadFileController> logger; 
        private readonly IWebHostEnvironment webHostEnvironment;
        public UploadFileController(ILogger<UploadFileController> logger, IWebHostEnvironment webHostEnvironment)
        {
            this.logger = logger;
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpPost]

        public async Task<IActionResult> Post()
        {
            try
            {
                var httpContent = HttpContext.Request;

                if (httpContent == null)
                    return BadRequest();

                if (httpContent.Form.Files.Count > 0)
                {
                    foreach (var file in httpContent.Form.Files)
                    {
                        var filePath = Path.Combine(@"C:\PriceGo_server_published\PriceGo2\ML_Tech_ConsoleApp1\ImageData");

                        if (!Directory.Exists(filePath))
                            Directory.CreateDirectory(filePath);


                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            System.IO.File.WriteAllBytes(Path.Combine(filePath, file.FileName), memoryStream.ToArray());

                        }
                        return Ok(new { fileName = file.FileName, message = "File uploaded successfully" });
                    }
                }
                return BadRequest("No file selected :(");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return new StatusCodeResult(500);
            }
        }
    }
}
