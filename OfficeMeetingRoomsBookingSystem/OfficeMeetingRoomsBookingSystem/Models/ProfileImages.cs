using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeMeetingRoomsBookingSystem.Models
{
    public class ProfileImages
    {
        public int ProfileImageID { get; set; }
        public string UserID { get; set; }
        public string ProfileImageFileName { get; set; }
        public IFormFile ProfileImage { get; set; }
    }
}
