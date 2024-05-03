using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeMeetingRoomsBookingSystem.Models
{
    public class MeetingRoomBooking
    {
        public int MeetingRoomBookingID { get; set; }
        public int MeetingRoomID { get; set; }
        public string MeetingRoomName { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string FullName { get; set; }
        public bool IsCurrentUserBooking { get; set; }
    }
}
