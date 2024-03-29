
$(function () {
    setMinDateInput();
});

//modify date input with id: date-picker to only choose the day greater than Today
function setMinDateInput() {
    $("#date-picker").prop("min", function () {
        return new Date().toJSON().split('T')[0];
    });
}


