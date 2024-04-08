
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
});
