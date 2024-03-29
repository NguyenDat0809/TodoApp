//this function will be used for ADD/EDIT depend on id parameter
//ADD - id is null
//Edit - id not null
function showModal(url, id) {
    //use ajax to get Modal object with information for html tag and maybe a task according to id is null or not
    $.ajax({
        type: "GET",
        url: '/Home/GetModalApi',
        datatype: "json",
        data: { selectedId: id },
        success: function (response) {
            $.ajax({
                type: 'POST',
                url: url,
                dataType: "html",
                data: { modal: response.data },
                success: function (data) {
                    $('#place-form').html(data);
                    $("#task-edit-modal").modal("show");
                    console.log("xong");
                }
            });
            // console.log(response);


            //});
            //console.log(response.data)
        },
        error: function (req, status, error) {
            console.log(status);
        }
    });
}