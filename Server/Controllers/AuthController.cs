using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string _connectionString = "Server=localhost;Database=PriceGo_db;Uid=root;Pwd=Admin;SslMode=None;";

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

                var getUserCmd = new MySqlCommand("SELECT UserId, Password FROM Users WHERE Email = @Email", connection);
                getUserCmd.Parameters.AddWithValue("@Email", model.Email);
                using (var reader = await getUserCmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var storedPassword = reader.GetString("Password");

                        
                        if (model.Password == storedPassword) 
                        {
                            return Ok(new { message = "Login successful!" });
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
}