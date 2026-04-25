using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public class DoctorRequestModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Specialization { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public double ConsultationFee { get; set; }
    }
    public class DoctorViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Specialization { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public double ConsultationFee { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
