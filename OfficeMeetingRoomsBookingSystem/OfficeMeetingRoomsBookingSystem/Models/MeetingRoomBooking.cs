using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeMeetingRoomsBookingSystem.Models
{
    public class MeetingRoomBooking
    {
        public int MeetingRoomID { get; set; }
        public string MeetingRoomName { get; set; }
        public int MeetingRoomFloor { get; set; }
        public int MeetingRoomCapacity { get; set; }
        public int MeetingRoomTakenSpaces { get; set; }
        public int MeetingRoomBookingID { get; set; }
        public string UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}
