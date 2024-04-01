

//modify date input with id: date-picker to only choose the day greater than Today
 function addMinDateInput() {
    $("#date-picker").prop("min", function () {
        return new Date().toISOString().split("T")[0];
    });
};

//set maximum col span when the table have no data
$(document).ready(function () {
    var maxColspan = $('table thead:first th').length; // Get the number of columns
    $('.table #no-data-table').prop('colspan', maxColspan); // Set the colspan attribute
});



