using System.ComponentModel.DataAnnotations;

namespace MCS_WEB_API.Models
{
    public class Appointment
    {
        public int AppointmentID { get; set; }

        public int AppointmentNo { get; set; }

        public DateOnly AppointmentDate {  get; set; }  

        public string? PatientTitle { get; set; }

        public string? PatientFirstName { get; set; }

        public string? PatientLastName { get; set; }

        public string? PatientContactNo { get; set; }

        [EmailAddress]
        public string? PatientEmail { get; set; }

        public DateOnly PatientDOB { get; set; }    


    }
}
