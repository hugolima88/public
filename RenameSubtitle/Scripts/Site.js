$(document).ready(function () {
    $("#lblMessage").val("");
    $('#upload-button').on('click', function (e) {
        if(!validateFields())
        {
            e.preventDefault(); // Cancel the submit
            return false;
        }
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
        validateInputFilesField();
    });

    $('#videoNameFormatSample').on('keyup change', function () {
        validateVideoNameField();
    });
});


function validateFields()
{
    var val1 = validateVideoNameField();
    var val2 = validateInputFilesField();
    return val1 && val2;
}

function validateVideoNameField() {
    var ret = true;

    var videoName = $("#videoNameFormatSample").val();
    if (videoName == "") {
        $("#div-step-one-cell-right").addClass("validate-error");
        $("#div-step-one-cell-right").removeClass("validate-success");
        ret = false;
    }
    else {
        $("#div-step-one-cell-right").removeClass("validate-error");
        $("#div-step-one-cell-right").addClass("validate-success");
    }
    return ret;
}

function validateInputFilesField() {
    var ret = true;

    var files = $("#uploadFile")[0].files;
    if (files.length <= 0) {
        $("#div-step-two-cell-right").addClass("validate-error");
        $("#div-step-two-cell-right").removeClass("validate-success");
        ret = false;
    }
    else {
        $("#div-step-two-cell-right").removeClass("validate-error");
        $("#div-step-two-cell-right").addClass("validate-success");
    }
    return ret;
}


