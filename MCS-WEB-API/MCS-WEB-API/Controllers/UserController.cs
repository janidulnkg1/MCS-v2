using MCS_WEB_API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MCS_WEB_API.Models;
using Serilog;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace MCS_WEB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context) 
        {
            _context = context;
        }


        [HttpGet("/api/v1/getUsers")]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            try
            {
                Log.Information("Retrieved All Users");
                return _context.users;



            }
            catch (Exception ex)
            {
                Log.Error("An error occurred when trying to retireve users: " + ex.Message);
                return BadRequest("An error occurred when trying to retireve users: " +ex);
            }
        }

        [HttpPut("/updateUser")]
        public async Task<ActionResult> Update(UserRegister newuser)
        {
            try
            {
                if (newuser == null)
                {
                    Log.Warning("Update operation failed: newuser is null.");
                    return BadRequest("User data is required.");
                }

                if (string.IsNullOrEmpty(newuser.Password))
                {
                    Log.Warning("Update operation failed: Password is null or empty.");
                    return BadRequest("Password is required.");
                }

                CreatePasswordHash(newuser.Password, out byte[] passwordHash, out byte[] passwordSalt);

                var user = await _context.users.FindAsync(newuser.userId);
                if (user == null)
                {
                    Log.Warning($"Update operation failed: User with UserId {newuser.userId} not found.");
                    return NotFound($"User with UserId {newuser.userId} not found.");
                }

                user.Username = newuser.Username ?? user.Username;
                user.Email = newuser.Email ?? user.Email;
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.Designation = newuser.Designation ?? user.Designation;

                _context.users.Update(user);
                await _context.SaveChangesAsync();
                Log.Information($"User details of user with UserId: {newuser.userId} have been updated!");
                return Ok("User updated successfully!");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while updating user details for user with UserId: {newuser.userId}!");
                return BadRequest($"Error: {ex.Message}");
            }
        }


        [HttpDelete("/deleteUser/{UserId:int}")]
        public async Task<ActionResult> Delete(int UserId)
        {
            try
            {
                var user = await _context.users.FindAsync(UserId);
                if (user == null)
                {
                    Log.Warning($"User with userId: {UserId} is not available!");
                    return NotFound($"User with userID: {UserId} not found.");
                }

                _context.users.Remove(user);
                await _context.SaveChangesAsync();

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
