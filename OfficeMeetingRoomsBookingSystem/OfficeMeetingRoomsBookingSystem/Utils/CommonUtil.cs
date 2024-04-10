using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeMeetingRoomsBookingSystem.Utils
{
    public static class CommonUtil
    {
        private static List<string> LoginActions = new List<string>
        {
            "LoginPage",
            "Login",
            "Register"
        };

        public static bool IsLoginAction(string viewName)
        {
            return LoginActions.Contains(viewName);
        }

        public static KeyValuePair<DateTime, DateTime> GetStartAndEndBookingDateTime(string selectedDateStr, string timeSlot)
        {
            DateTime selectedDate = DateTime.Parse(selectedDateStr);
            string[] slotParts = timeSlot.Split("-").ToArray();
            string[] startTme = slotParts[0].Split(":").ToArray();
            string[] endTime = slotParts[1].Split(":").ToArray();
            int startHour = Convert.ToInt32(startTme[0]);
            int endHour = Convert.ToInt32(endTime[0]);

            DateTime startDateTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, startHour, 0, 0);

            if (startHour == 23)
                selectedDate = selectedDate.AddDays(1);

            DateTime endDateTime = new DateTime(selectedDate.Year, selectedDate.Month, selectedDate.Day, endHour, 0, 0);

            KeyValuePair<DateTime, DateTime> bookingTime = new KeyValuePair<DateTime, DateTime>(startDateTime, endDateTime);

            return bookingTime;
        }
    }
}
