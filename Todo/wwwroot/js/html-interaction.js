
//$(function(){
//    addMinDateInput();
//});

//modify date input with id: date-picker to only choose the day greater than Today
 function addMinDateInput() {
    $("#date-picker").prop("min", function () {
        return new Date().toISOString().split("T")[0];
    });
};


