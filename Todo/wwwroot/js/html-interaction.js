

//modify date input with id: date-picker to only choose the day greater than Today
function addMinDateInput() {
    $("#date-picker").prop("min", function () {
        var now = new Date();
        var date = now.toISOString().split('T')[0]; //take datetime following format yyyy-MM-dd
        var time = now.toTimeString().split(':')[0] + ':' + now.toTimeString().split(':')[1]; // take hour and minute
        return date + 'T' + time; //combine 
    });
};


//set maximum col span when the table have no data
$(document).ready(function () {
    var maxColspan = $('table thead:first th').length; // Get the number of columns
    $('.table #no-data-table').prop('colspan', maxColspan); // Set the colspan attribute
});



