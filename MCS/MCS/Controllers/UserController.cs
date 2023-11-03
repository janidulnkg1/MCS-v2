using MCS.Data;
using MCS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Cryptography;

namespace MCS.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    //[Authorize(Roles = "ADMIN")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public UserController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("/getUsers")]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            try
            {
                Log.Information("Retrieved all available users!");
                return _dbContext.Users;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while checking for all users!");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPut("/updateUser")]
        public async Task<ActionResult> Update(UserRegister newuser)
        {
            if (newuser == null)
            {
                Log.Error("Invalid User Details provided!");
                return BadRequest("Invalid user data. Please provide valid user information.");
            }

            try
            {
                if (string.IsNullOrEmpty(newuser.Password))
                {
                    Log.Error("Password is empty!");
                    return BadRequest("Password cannot be empty.");
                }

                CreatePasswordHash(newuser.Password, out byte[] passwordHash, out byte[] passwordSalt);

                if (passwordHash == null || passwordSalt == null)
                {
                    Log.Error("Error creating password hash and salt!");
                    return BadRequest("Error creating password hash and salt.");
                }

                var user = new User
                {
                    UserId = newuser.userId,
                    Username = newuser.Username,
                    Email = newuser.Email,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Designation = newuser.Designation
                };

                if (_dbContext != null)
                {
                    _dbContext.Users.Update(user);
                    await _dbContext.SaveChangesAsync();
                    Log.Information($"User details of user with Userid: {newuser.userId} have been updated!");
                    return Ok("User updated successfully!");
                }
                else
                {
                    Log.Error("Null Database Context!");
                    return BadRequest("Database context is null. Unable to update user.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while updating user details for user with Userid: {newuser.userId}!");
                return BadRequest($"Error: {ex.Message}");
            }
        }


        [HttpDelete("/deleteUser/{UserId:int}")]
        public async Task<ActionResult> Delete(int UserId)
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(UserId);
                if (user == null)
                {
                    Log.Warning($"User with userId: {UserId} is not available!");
                    return NotFound($"User with userID: {UserId} not found.");
                }

                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();

                Log.Warning($"User with userId: {UserId} has been removed!");
                return Ok($"User {UserId} has been removed successfully!");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while deleting user with Userid: {UserId}!");
                return BadRequest($"Error: {ex.Message}");
            }
        }


        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
