
$(document).ready(function () {
    $("#ddFloor").change(function () {
        ClearMeetingRooms();
        var floor = $(this).val();
        if (floor !== "") {
            $("#txtDay").prop('disabled', false);
        }
        else {
            $("#txtDay").prop('disabled', true);
        }
        $("#ddTimeSlot").prop('disabled', true);
        $("#ddTimeSlot").val('');
        $("#txtDay").val('');
    });

    $("#txtDay").change(function () {
        ClearMeetingRooms();
        var day = $(this).val();
        if (day !== "") {
            $("#ddTimeSlot").prop('disabled', false);

            $.ajax({
                url: "/Home/GetTimeSlots",
                type: "GET",
                data: { selectedDateStr: day },
                success: function (response) {
                    $("#ddTimeSlot").empty().append('<option value="">Select Time Slot</option>');
                    $.each(response, function (index, slot) {
                        $("#ddTimeSlot").append('<option value="' + slot + '">' + slot + '</option>');
                    });
                }
            });
        }
        else {
            $("#ddTimeSlot").prop('disabled', true);
        }
        $("#ddTimeSlot").val('');
    });

    $("#ddTimeSlot").change(function () {
        ClearMeetingRooms();
        var floor = $("#ddFloor").val();
        var day = $("#txtDay").val();
        var timeSlot = $(this).val();
        if (floor !== "" && day !== "" && timeSlot !== "") {

            $.ajax({
                url: "/Home/ShowMeetingRooms",
                type: "GET",
                data: { floor: floor, selectedDateStr: day, timeSlot: timeSlot },
                success: function (response) {
                    $("#meetingRoomsContainer").html(response);
                }
            });
        }
    });

    function ClearMeetingRooms() {
        $("#meetingRoomsContainer").empty();
    }

    function OpenMeetingRoomBookingPopup() {
        $("#popupForm").show();
        $("#overlay").show();
        $("#navbar-menu").attr("onclick", "return false;");
        $("header").find(".navbar, .navbar-brand, .navbar-nav, .nav-link, .nav-item i").addClass("disabled-link");
    }

    function CloseMeetingRoomBookingPopup() {
        $("#popupForm").hide();
        $("#overlay").hide();
        $("#navbar-menu").removeAttr("onclick");
        $("header").find(".navbar, .navbar-brand, .navbar-nav, .nav-link, .nav-item i").removeClass("disabled-link");
    }

    $(document).on('click', '.room-container', function () {
        var roomId = $(this).attr('id');
        var roomName = $(this).data('roomName');
        var day = $("#txtDay").val();
        var timeSlot = $("#ddTimeSlot").val();

        $.ajax({
            url: "/Home/OpenMeetingRoomBookingForm",
            type: "GET",
            data: {
                roomId: roomId,
                roomName: roomName,
                selectedDateStr: day,
                timeSlot: timeSlot
            },
            success: function (response) {
                $("#meetingRoomBookingContainer").html(response);
                OpenMeetingRoomBookingPopup();
            }
        });
    });

    $(document).on('click', '#btnSaveBooking', function () {
        var meetingRoomID = $("#hdnBookingMeetingRoomID").val();
        var meetingRoomName = $("#hdnBookingMeetingRoomName").val();
        var startDateTime = $("#txtBookingStartDateTime").val();
        var endDateTime = $("#txtBookingEndDateTime").val();

        $.ajax({
            url: "/Home/BookMeetingRoom",
            type: "POST",
            data: {
                meetingRoomID: meetingRoomID,
                meetingRoomName: meetingRoomName,
                startDateTime: startDateTime,
                endDateTime: endDateTime
            },
            success: function (response) {
                if (response.success) {
                    $('#btnClosePopUp').trigger('click');
                } else {
                    $("#bookingRoomError").text(response.message).show();
                    $("#bookingRoomForm").hide();
                }
            }
        });
    });

    $(document).on('click', '#btnClosePopUp', function () {
        CloseMeetingRoomBookingPopup();
        ClearMeetingRooms();
        var floor = $("#ddFloor").val();
        var day = $("#txtDay").val();
        var timeSlot = $("#ddTimeSlot").val();
        if (floor !== "" && day !== "" && timeSlot !== "") {

            $.ajax({
                url: "/Home/ShowMeetingRooms",
                type: "GET",
                data: { floor: floor, selectedDateStr: day, timeSlot: timeSlot },
                success: function (response) {
                    $("#meetingRoomsContainer").html(response);
                }
            });
        }
    });
});
