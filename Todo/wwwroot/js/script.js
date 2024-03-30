
//this function will be used for ADD/EDIT depend on id parameter
//ADD - id is null
//Edit - id not null
function showModal(url, title) {
    //use ajax to get Modal object with information for html tag and maybe a task according to id is null or not
    $.ajax({
        type: "GET",
        url: url,
        success: function (res) {
            $('#form-modal .modal-body').html(res);
            $("#form-modal .modal-title").html(title);
            $('#form-modal').modal('show');
            console.log("xong");
        },
        error: function (req, status, error) {
            console.log(status);
        }
    });
}

//submit edit/add form validation check
checkValidate = form => {
    var formData = new FormData(form); // Tạo đối tượng FormData từ form
    try {

        $.ajax({
            type: "POST",
            url: 'Home/AddOrEdit',
            data: formData, // Sử dụng đối tượng FormData đã tạo
            processData: false,
            contentType: false, // Thiết lập contentType là false để jQuery tự động xác định nó dựa trên đối tượng FormData
            success: function (res) {
                if (res.isValid) {
                    $('#view-all').html(res.html);

                    $('#form-modal .modal-body').html("");
                    $("#form-modal .modal-title").html("");
                    $('#form-modal').modal('hide');
                } else {
                    $('#form-modal .modal-body').html(res.html);
                }
            },
            error: function (xhr, status, error) {
                console.log(xhr.responseText);
            }
        });
        return false; // để ngăn sự kiện submit mặc định của form
    } catch (e) {
        console.log(e);
    }
}


