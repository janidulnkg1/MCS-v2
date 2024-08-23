using MCS_WEB_API.Data;
using MCS_WEB_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MCS_WEB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DoctorController(AppDbContext dbContext) 
        {
            _context = dbContext;
        }

        [HttpGet("/api/v1/getDoctors")]
        public ActionResult<IEnumerable<Doctor>> GetDoctors()
        {
            try
            {
                Log.Information("Retrieved All Doctor Details");
                return _context.doctors;

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred when trying to retrieve all Doctors Details: " + ex.Message);
                return BadRequest("An error occurred when trying to all Doctors Details: " + ex);
            }
        }

        [HttpPut("/api/v1/updateDoctor")]
        public async Task<ActionResult> Update(Doctor doctor)
        {
            try
            {


                var doctors = await _context.doctors.FindAsync(doctor.DoctorID);
                if (doctors == null)
                {
                    Log.Warning($"Update operation failed: Doctor with Id {doctor.DoctorID} not found.");
                    return NotFound($"Doctor with id {doctor.DoctorID} not found.");
                }

                _context.doctors.Update(doctors);
                await _context.SaveChangesAsync();
                Log.Information($"Detailsetails of doctor Id : {doctor.DoctorID} have been updated!");
                return Ok("Doctor Details updated successfully!");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while updating Doctor details for doctor ID : {doctor.DoctorID}!");
                return BadRequest($"Error: {ex.Message}");
            }
        }


        [HttpDelete("/api/v1/deleteDoctor/{DoctorId:int}")]
        public async Task<ActionResult> Delete(int DoctorId)
        {
            try
            {
                var doctor = await _context.doctors.FindAsync(DoctorId);
                if (doctor == null)
                {
                    Log.Warning($"Doctor with Id: {DoctorId} is not available!");
                    return NotFound($"Doctor with Id: {DoctorId} not found.");
                }

                _context.doctors.Remove(doctor);
                await _context.SaveChangesAsync();

                Log.Warning($"Doctor with Id: {DoctorId} has been removed!");
                return Ok($"Doctor {DoctorId} has been removed successfully!");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while deleting doctor id: {DoctorId}!");
                return BadRequest($"An error occurred while deleting doctor id: {DoctorId}! Error: {ex.Message}");
            }
        }



    }
}
