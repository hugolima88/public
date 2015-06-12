$(document).ready(function () {
    $('#upload-button').on('click', function (e) {
        //if (files.length > 0) {
        //    if (window.FormData !== undefined) {
        //        var data = new FormData();
        //        for (var x = 0; x < files.length; x++) {
        //            data.append("file" + x, files[x]);
        //        }
               
        //    } else {
        //        alert("This browser doesn't support HTML5 file uploads!");
        //    }
        //}

        //e.preventDefault(); // Cancel the submit
        //return false;
    });
    $('#btn-upload-file').on('click', function (e) {
        $("#uploadFile").click();
    });

    $('#uploadFile').on('change', function () {
        var files = $("#uploadFile")[0].files;

        if (files.length > 0) {
            var data = "";

            for (var x = 0; x < files.length; x++) {
                data = files[x].name + " " + data;
            }
        }
        $('#btn-upload-file').val(data);
    });
});
