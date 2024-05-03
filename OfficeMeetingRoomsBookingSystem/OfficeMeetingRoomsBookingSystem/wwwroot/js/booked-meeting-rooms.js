
$(".try-delete-icon").click(function (e) {
    e.stopPropagation();
    ResetDeleteIcons(0);
    $(this).hide();
    $(this).siblings(".bin-icon, .cancel-icon").fadeIn(200);
});

$(document).click(function () {
    ResetDeleteIcons(200);
});

$(".cancel-icon").click(function () {
    ResetDeleteIcons(200);
});

$(".bin-icon").click(function (e) {
    e.stopPropagation();
    var bookingID = $(this).closest('tr').attr('id');

    $.ajax({
        type: 'POST',
        url: '/Home/DeleteBooking',
        data: { bookingID: bookingID },
        success: function (response) {
            window.location.reload();
        }
    });
});

function ResetDeleteIcons(timeMS) {
    $(".bin-icon, .cancel-icon").fadeOut(timeMS, function () {
        $(this).siblings(".try-delete-icon").show();
    });
}
