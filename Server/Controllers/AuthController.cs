using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> logger;
        private readonly IWebHostEnvironment webHostEnvironment;
        public AuthController(ILogger<AuthController> logger, IWebHostEnvironment webHostEnvironment)
        {
            this.logger = logger;
            this.webHostEnvironment = webHostEnvironment;
        }
        private readonly string _connectionString = "Server=localhost;Database=PriceGo_db;Uid=root;Pwd=Admin;SslMode=None;";
        private string _userId;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Email and Password are required.");
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var checkEmailCmd = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @Email", connection);
                checkEmailCmd.Parameters.AddWithValue("@Email", model.Email);
                var userCount = (long)await checkEmailCmd.ExecuteScalarAsync();

                if (userCount > 0)
                {
                    return BadRequest("Email already exists.");
                }

                var insertUserCmd = new MySqlCommand("INSERT INTO Users (Email, Password) VALUES (@Email, @Password)", connection);
                insertUserCmd.Parameters.AddWithValue("@Email", model.Email);
                insertUserCmd.Parameters.AddWithValue("@Password", model.Password);
                await insertUserCmd.ExecuteNonQueryAsync();

                return Ok(new { message = "User registered successfully!" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Email and Password are required.");
            }

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var getUserCmd = new MySqlCommand("SELECT UserId, Password, photo FROM users WHERE Email = @Email", connection);
                getUserCmd.Parameters.AddWithValue("@Email", model.Email);

                using (var reader = await getUserCmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var userId = reader.GetInt32("UserId");
                        var storedPassword = reader.GetString("Password");
                        var photoUrl = reader.IsDBNull("photo") ? null : reader.GetString("photo");


                        if (model.Password == storedPassword)
                        {
                            return Ok(new
                            {
                                message = "Login successful!",
                                userId = userId,
                                email = model.Email,
                                photoUrl = photoUrl
                            });
                        }
                        else
                        {
                            return Unauthorized(new { message = "Invalid credentials." });
                        }
                    }
                    else
                    {
                        return Unauthorized(new { message = "User not found." });
                    }
                }
            }
        }

        [HttpGet("getUserProfile")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var getUserCmd = new MySqlCommand("SELECT Email, photo FROM users WHERE UserId = @UserId", connection);
                getUserCmd.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await getUserCmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var email = reader.GetString("Email");
                        var photoUrl = reader.IsDBNull("photo") ? null : reader.GetString("photo");

                        return Ok(new
                        {
                            email = email,
                            photoUrl = photoUrl
                        });
                    }
                    else
                    {
                        return NotFound(new { message = "User not found." });
                    }
                }
            }
        }

        [HttpPost("ChangeAvatar")]
        public async Task<IActionResult> ChangeAvatar(int userId)
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
                        var filePath = Path.Combine(@"C:\PriceGo_server_published\PriceGo2\ML_Tech_ConsoleApp1\ImageData\Avatar");

                        if (!Directory.Exists(filePath))
                            Directory.CreateDirectory(filePath);

                        var fullFilePath = Path.Combine(filePath, file.FileName);

                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            System.IO.File.WriteAllBytes(fullFilePath, memoryStream.ToArray());
                        }   

                        using (var connection = new MySqlConnection(_connectionString))
                        {
                            await connection.OpenAsync();

                            var updateCmd = new MySqlCommand("UPDATE users SET photo = @PhotoUrl WHERE UserId = @UserId", connection);
                            updateCmd.Parameters.AddWithValue("@PhotoUrl", fullFilePath);
                            updateCmd.Parameters.AddWithValue("@UserId", _userId);

                            await updateCmd.ExecuteNonQueryAsync();
                        }

                        return Ok(new { fileName = file.FileName, message = "File uploaded successfully", filePath = fullFilePath });
                    }
                }
                return BadRequest("No file selected :(");



            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500);
            }
        }

        public class RegisterModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class LoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class AvatarModel
        {
            public string Photo { get; set; }
        }
    }
}