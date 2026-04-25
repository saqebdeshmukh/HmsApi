using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModels
{
    public class ReceptionistRequestModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class ReceptionistViewModel
    {
        public string PhoneNo { get; set; }
        public string EmailId { get; set; }
        public string Name { get; set; }
        public DateTime Created {  get; set; }
    }
}
