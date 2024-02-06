using MCS.Data;
using MCS.Models;
using MCS.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace MCS.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        public static User user = new User();
        private readonly IConfiguration _configuration;
        private readonly IAppDbContext _dbContext;
        private readonly IUserService _userService;

        public AuthController(IConfiguration configuration, IAppDbContext appDbContext, IUserService userService)
        {
            _configuration = configuration;
            _dbContext = appDbContext;
            _userService = userService;
        }

        [HttpPost("/register")]
        //[Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<User>> Register(UserRegister newuser)
        {
            try
            {
                if (newuser == null)
                {
                    return BadRequest("Invalid user data");
                }

                if (string.IsNullOrWhiteSpace(newuser.Password) || string.IsNullOrWhiteSpace(newuser.Username) || string.IsNullOrWhiteSpace(newuser.Email))
                {
                    return BadRequest("Username, password, or email cannot be empty");
                }

                
                var userExists = await _dbContext.Users.AnyAsync(u => u.Email == newuser.Email);
                if (userExists)
                {
                    return Conflict("User with this email already exists");
                }

                
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

                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();
                Log.Information($"User {newuser.Username} Added Successfully!");
                return Ok($"{newuser.Designation} Account Created Successfully!");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while registering a new user!");
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while trying to Register. Details: {ex.Message}");
            }

        }

        [HttpPost("/login")]
        public async Task<ActionResult<User>> Login(UserLogin request)
        {
            try
            {
                
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

                
                if (user == null)
                {
                    Log.Warning($"User {request.Username} Not Found!");
                    return BadRequest("User not found.");
                }

                
                if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                {
                    Log.Warning($"User {request.Password} Incorrect!");
                    return BadRequest("Wrong password.");
                }

                
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
        

        public Task<IActionResult> Logout()
        {
            try
            {
                HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                Log.Information("User Log out Sucessful!");
                return Task.FromResult<IActionResult>(Ok("Logout successful."));
            }
            catch (Exception ex)
            {
                
                Log.Error(ex, "An error occurred while logging out!");
                return Task.FromResult<IActionResult>(BadRequest("Logout failed."));
            }
        }

        private string? CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>();

            if (!string.IsNullOrEmpty(user.Designation))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Designation));
            }

            string? encodedjwtKey = _configuration.GetSection("JWT:Key").Value;

            if (string.IsNullOrEmpty(encodedjwtKey))
            {
                return null; 
            }

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(encodedjwtKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
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

        private bool VerifyPasswordHash(string? password, byte[]? passwordHash, byte[]? passwordSalt)
        {
            if (password == null || passwordHash == null || passwordSalt == null)
            {
                return false; 
            }

            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                // Ensure the passwordHash length matches the computedHash length
                if (computedHash.Length != passwordHash.Length)
                {
                    return false; 
                }

                // Compare byte arrays for equality
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i])
                    {
                        return false; 
                    }
                }

                return true; 
            }
        }

    }
}
