using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeMeetingRoomsBookingSystem.Models
{
    public class MeetingRoomDetails
    {
        public int MeetingRoomID { get; set; }
        public string MeetingRoomName { get; set; }
        public int MeetingRoomTakenSpaces { get; set; }
        public int MeetingRoomCapacity { get; set; }
    }
}
