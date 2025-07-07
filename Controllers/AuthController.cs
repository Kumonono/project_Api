using Microsoft.AspNetCore.Mvc;
using api.Services;
using api.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseService _db;
        private readonly JwtService _jwt;
        private readonly MfaService _mfa;
        private readonly Dictionary<string, (string Code, DateTime Expiration)> _resetCodes = new();

        public AuthController(DatabaseService db, JwtService jwt, MfaService mfa)
        {
            _db = db;
            _jwt = jwt;
            _mfa = mfa;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] api.Models.LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email and password are required.");
            }

            var user = (await _db.GetUsers()).FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
                return Unauthorized("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                return Unauthorized("Invalid credentials");

            var mfaCode = _mfa.GenerateCode(user.Email);

            return Ok(new { message = "MFA code sent. Please verify to complete login." });
        }

        [HttpPost("verify-mfa")]
        public async Task<IActionResult> VerifyMfa([FromBody] MfaRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Code))
            {
                return BadRequest("Email and MFA code are required.");
            }

            var user = (await _db.GetUsers()).FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
                return Unauthorized("Invalid email");

            if (!_mfa.ValidateCode(request.Email, request.Code))
            {
                return Unauthorized("Invalid MFA code");
            }


            _mfa.ClearCode(request.Email);


            var token = _jwt.GenerateToken(user.UserId!);
            return Ok(new { token });
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] EmailRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest("Email is required.");

            var user = (await _db.GetUsers()).FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
                return NotFound("No user found with this email.");

            var code = new Random().Next(100000, 999999).ToString();
            var expiration = DateTime.UtcNow.AddMinutes(10);

            // Frissítjük az adatbázist
            user.ResetCode = code;
            user.ResetCodeExpiration = expiration;
            await _db.UpdateUserAsync(user.UserId, user);

            _mfa.SendResetCodeEmail(request.Email, code);

            return Ok(new { message = "Reset code sent to email. It is valid for 10 minutes." });
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] api.Models.ResetPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.NewPassword))
                return BadRequest("All fields are required.");

            var user = (await _db.GetUsers()).FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
                return NotFound("User not found.");

            if (user.ResetCode == null || user.ResetCodeExpiration == null)
                return Unauthorized("No reset request found.");

            if (DateTime.UtcNow > user.ResetCodeExpiration || user.ResetCode != request.Code)
                return Unauthorized("Invalid or expired code.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            user.ResetCode = null;
            user.ResetCodeExpiration = null;

            await _db.UpdateUserAsync(user.UserId, user);

            return Ok(new { message = "Password updated successfully." });
        }


    }
}