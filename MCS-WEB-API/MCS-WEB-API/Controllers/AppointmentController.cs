using MCS_WEB_API.Data;
using MCS_WEB_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MCS_WEB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext dbcontext)
        {
            _context = dbcontext;
        }

        [HttpGet("/api/v1/getAppointments")]
        public ActionResult<IEnumerable<Appointment>> GetAppointments()
        {
            try
            {
                Log.Information("Retrieved All Appointments");
                return _context.appointments;

            }
            catch (Exception ex)
            {
                Log.Error("An error occurred when trying to retrieve all appointments: " + ex.Message);
                return BadRequest("An error occurred when trying to all appointments: " + ex);
            }
        }

        [HttpPut("/api/v1/updateAppointment")]
        public async Task<ActionResult> Update(Appointment appointment)
        {
            try
            {


                var appointments = await _context.appointments.FindAsync(appointment.AppointmentID);
                if (appointments == null)
                {
                    Log.Warning($"Update operation failed: Appointment with Id {appointment.AppointmentID} not found.");
                    return NotFound($"Appointment with id {appointment.AppointmentID} not found.");
                }

                _context.appointments.Update(appointment);
                await _context.SaveChangesAsync();
                Log.Information($"Appointment details of appointment No : {appointment.AppointmentID} have been updated!");
                return Ok("Appointment Details updated successfully!");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while updating Appointment details for user for : {appointment.AppointmentID}!");
                return BadRequest($"Error: {ex.Message}");
            }
        }


        [HttpDelete("/api/v1/deleteAppointment/{AppointmentId:int}")]
        public async Task<ActionResult> Delete(int AppointmentId)
        {
            try
            {
                var appointment = await _context.appointments.FindAsync(AppointmentId);
                if (appointment == null)
                {
                    Log.Warning($"Appointment with Id: {AppointmentId} is not available!");
                    return NotFound($"Appointment with Id: {AppointmentId} not found.");
                }

                _context.appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                Log.Warning($"Appointment with Id: {AppointmentId} has been removed!");
                return Ok($"Appointment {AppointmentId} has been removed successfully!");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while deleting appointment id: {AppointmentId}!");
                return BadRequest($"An error occurred while deleting appointment id: {AppointmentId}! Error: {ex.Message}");
            }
        }


    }
}
