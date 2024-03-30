

//modify date input with id: date-picker to only choose the day greater than Today
$(window).on("load", function () {
    $("#date-picker").prop("min", function () {
        return new Date().toJSON().Split("T")[0];
    });
});


