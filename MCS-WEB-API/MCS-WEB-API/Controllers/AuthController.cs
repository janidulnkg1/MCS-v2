using MCS_WEB_API.Data;
using MCS_WEB_API.Middleware;
using MCS_WEB_API.Models;
using MCS_WEB_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace MCS_WEB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        readonly AppDbContext _context;
        readonly IConfiguration _config;
        readonly IUserService _userService;


        public AuthController(AppDbContext context, IConfiguration config, IUserService userService)
        {
            _context = context;
            _config = config;
            _userService = userService;
        }

        [HttpGet("/getme")]
        [Authorize(Roles = "ADMIN,USER,DOCTOR,RECEPTIONIST")]
        public ActionResult<string> GetMe()
        {
            try
            {
                var userdetails = new
                {
                    UserName = _userService.GetMyUsername(),
                    Designation = _userService.GetMyDesignation(),
                };

                Log.Information($"{userdetails.Designation} User {userdetails.UserName} logged in!");
                return Ok(userdetails);

            }
            catch(Exception ex)
            {
                Log.Error("An error occurred while checking for logged user:" + ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpPost("/register")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> Register(UserRegister newuser)
        {
            try
            {
                CreatePasswordHash(newuser.Password, out byte[] passwordHash, out byte[] passwordSalt);

                var user = new User
                {
                    UserId = newuser.userId,
                    Username = newuser.Username,
                    Email = newuser.Email,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Designation = newuser.Designation
                };

                await _context.users.AddAsync(user);
                await _context.SaveChangesAsync();
                Log.Information($"User Account {user.Username} Added Successfully!");
                return Ok($"{newuser.Designation} Account Created Successfully!");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while registering new user!");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while trying to Register.");
            }
        }

        [HttpPost("/login")]
        public async Task<ActionResult<User>> Login(UserLogin request)
        {
            try
            {
                // Check if the user exists in the database
                var user = await _context.users.FirstOrDefaultAsync(u => u.Username == request.Username);

                // Check if the user exists
                if (user == null)
                {
                    Log.Warning($"User {request.Username} Not Found!");
                    return BadRequest("User not found.");
                }

                // Verify the password
                if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                {
                    Log.Warning($"User {request.Password} Incorrect!");
                    return BadRequest("Wrong password.");
                }

                // Create and return a JWT token
                var token = CreateToken(user);

                var response = new
                {
                    Token = token,
                    Designation = user.Designation
                };
                Log.Information($"User: {request.Username} login Successful!");
                return Ok(response);
            }
            catch (Exception ex)
            {

                Log.Error(ex, "An error occurred while logging in!");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while trying to log in.");
            }
        }

        [HttpPost("/logout")]
        [Authorize(Roles = "USER,ADMIN")]
        [CheckToken]

        public async Task<IActionResult> Logout()
        {
            try
            {
                // Clear the token from the Authorization header, effectively "removing" it
                HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                Log.Information("User Log out Sucessful!");
                return Ok("Logout successful.");
            }
            catch (Exception ex)
            {
                // Handle exception
                Log.Error(ex, "An error occurred while logging out!");
                return BadRequest("Logout failed.");
            }
        }



        private string? CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>();
            if (!string.IsNullOrEmpty(user.Username))
            {
                claims.Add(new Claim(ClaimTypes.Name, user.Username));
            }

            if (!string.IsNullOrEmpty(user.Designation))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Designation));
            }


            //claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _config.GetSection("JWT:Key").Value));
            if (key == null)
            {
                return null;
            }

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(45),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        // Helper method to verify password hash
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }




    }
}
