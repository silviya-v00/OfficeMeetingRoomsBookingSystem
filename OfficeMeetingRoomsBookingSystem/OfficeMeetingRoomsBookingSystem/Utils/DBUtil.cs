using Microsoft.Data.SqlClient;
using OfficeMeetingRoomsBookingSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeMeetingRoomsBookingSystem.Utils
{
    public class DBUtil
    {
        private string _connectionString;
        public DBUtil(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<int> GetMeetingRoomFloors()
        {
            List<int> meetingRoomFloors = new List<int>();
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT DISTINCT MeetingRoomFloor
                               FROM dbo.MeetingRooms
                               ORDER BY MeetingRoomFloor";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    if (dataReader["MeetingRoomFloor"] is int)
                        meetingRoomFloors.Add((int)dataReader["MeetingRoomFloor"]);
                }

                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return meetingRoomFloors;
        }

        public List<MeetingRoomDetails> GetMeetingRoomDetailsByFloorAndDateTime(int floorNum, DateTime startDateTime, DateTime endDateTime)
        {
            List<MeetingRoomDetails> meetingRoomDetails = new List<MeetingRoomDetails>();
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"SELECT a.MeetingRoomID, a.MeetingRoomName, ISNULL(b.MeetingRoomTakenSpaces, 0) as MeetingRoomTakenSpaces, a.MeetingRoomCapacity
                               FROM dbo.MeetingRooms a
                               LEFT OUTER JOIN (SELECT a.MeetingRoomID, COUNT(*) as MeetingRoomTakenSpaces
				                                FROM dbo.MeetingRoomBooking a
				                                WHERE a.StartDateTime = @StartDateTime AND a.EndDateTime = @EndDateTime
				                                GROUP BY a.MeetingRoomID) b ON a.MeetingRoomID = b.MeetingRoomID
                               WHERE a.MeetingRoomFloor = @FloorNum
                               ORDER BY a.MeetingRoomName";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@FloorNum", System.Data.SqlDbType.Int).Value = floorNum;
                command.Parameters.Add("@StartDateTime", System.Data.SqlDbType.DateTime).Value = startDateTime;
                command.Parameters.Add("@EndDateTime", System.Data.SqlDbType.DateTime).Value = endDateTime;
                SqlDataReader dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    MeetingRoomDetails meetingRoom = new MeetingRoomDetails();

                    if (dataReader["MeetingRoomID"] is int)
                        meetingRoom.MeetingRoomID = (int)dataReader["MeetingRoomID"];

                    meetingRoom.MeetingRoomName = dataReader["MeetingRoomName"].ToString();

                    if (dataReader["MeetingRoomTakenSpaces"] is int)
                        meetingRoom.MeetingRoomTakenSpaces = (int)dataReader["MeetingRoomTakenSpaces"];

                    if (dataReader["MeetingRoomCapacity"] is int)
                        meetingRoom.MeetingRoomCapacity = (int)dataReader["MeetingRoomCapacity"];

                    meetingRoomDetails.Add(meetingRoom);
                }

                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return meetingRoomDetails;
        }

        public string BookMeetingRoom(string userID, int meetingRoomID, string meetingRoomName, DateTime startDateTime, DateTime endDateTime)
        {
            string errMsg = "";
            var sqlConn = new SqlConnection(_connectionString);
            sqlConn.Open();

            try
            {
                string SQL = @"
                                DECLARE @ErrMessage nvarchar(max)
                                DECLARE @MeetingRoomTakenSpaces int
                                DECLARE @MeetingRoomCapacity int
                                DECLARE @HasBooking int

                                SELECT @HasBooking = COUNT(*)
                                FROM dbo.MeetingRoomBooking
                                WHERE UserID = @CurrentUserID
                                    AND StartDateTime = @StartDateTime
                                    AND EndDateTime = @EndDateTime

                                SELECT @MeetingRoomTakenSpaces = ISNULL(b.MeetingRoomTakenSpaces, 0), @MeetingRoomCapacity = a.MeetingRoomCapacity
                                FROM dbo.MeetingRooms a
                                LEFT OUTER JOIN (SELECT a.MeetingRoomID, COUNT(*) as MeetingRoomTakenSpaces
				                                 FROM dbo.MeetingRoomBooking a
				                                 WHERE a.StartDateTime = @StartDateTime AND a.EndDateTime = @EndDateTime
				                                 GROUP BY a.MeetingRoomID) b ON a.MeetingRoomID = b.MeetingRoomID
                                WHERE a.MeetingRoomID = @MeetingRoomID

                                IF @HasBooking > 0
                                    SET @ErrMessage = 'You already have a booking during the specified times.'
                                ELSE
                                BEGIN
                                    IF @MeetingRoomTakenSpaces >= @MeetingRoomCapacity
                                        SET @ErrMessage = 'Room ' + @MeetingRoomName + ' is fully booked during the specified times.'
                                    ELSE
	                                BEGIN	
		                                INSERT INTO dbo.MeetingRoomBooking (UserID, MeetingRoomID, StartDateTime, EndDateTime)
		                                VALUES (@CurrentUserID, @MeetingRoomID, @StartDateTime, @EndDateTime)
	                                END
                                END

                                SELECT @ErrMessage as ErrMessage";

                SqlCommand command = new SqlCommand(SQL, sqlConn);
                command.Parameters.Add("@StartDateTime", System.Data.SqlDbType.DateTime).Value = startDateTime;
                command.Parameters.Add("@EndDateTime", System.Data.SqlDbType.DateTime).Value = endDateTime;
                command.Parameters.Add("@MeetingRoomID", System.Data.SqlDbType.Int).Value = meetingRoomID;
                command.Parameters.Add("@MeetingRoomName", System.Data.SqlDbType.NVarChar).Value = meetingRoomName;
                command.Parameters.Add("@CurrentUserID", System.Data.SqlDbType.NVarChar).Value = userID;
                SqlDataReader dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    errMsg = dataReader["ErrMessage"].ToString();
                }

                dataReader.Close();
            }
            finally
            {
                sqlConn.Close();
            }

            return errMsg;
        }
    }
}
