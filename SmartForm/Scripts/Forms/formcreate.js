
var form = {};
var formDatas = {};
var formTemplateFields = [];
var formDataVMs = [];

//$("#FormTemplateId").change(function () {
$(".getForm").click(function () { 

    //var id = $(this).children("option:selected").val();
    //form.FormTemplateId = id;


    id = $(this).data('id');
    name = $(this).data('name');

    if (!id) {
        return;
    }
    $.ajax({
        type: "GET",
        contentType: "application/json",
        url: '/Forms/GetFormFields',
        data: {
            id:id
        }
    }).done(function (response) {

        formTemplateFields = JSON.parse(response);
        formTemplateFields.sort((a, b) => (a.SortOrder > b.SortOrder) ? 1 : -1)
        var htmlString='<input type="text" id="formTemplateId" hidden value="'+id+'">';
        //var htmlString = '<div class="card">' +
        //    '<div class="card-header"><span class="card-header-text">Enter FormData</span></div>'+
        //    '<div class="card-body" > ';

        formTemplateFields.forEach(function (el) {
            htmlString += generateFormFieldHtml(el);
        })
        //htmlString += '<button class="btn btn-primary">Save</button>';
        //htmlString += '</div></div>';

        $("#formBody").empty();
        $("#formBody").append(htmlString);
        $("#exampleModalLabel").text(name);

    }).fail(function (XMLHttpReques, textStatus, errorThrown) {
            alert("error happened in ajax request");
        });

})


$(document).on("click", "#btnSave", function () {

    var arrayOfGuid = formTemplateFields.map(function (el) {
        return el.Guid;
    })

    arrayOfGuid.forEach(function (el) {
        formDatas[el] = $("[data-id=" + el + "]").val();
    })

    var formValues = Object.values(formDatas);
    if (checkEmptyArray(formValues)) {
        alert("Every fields must be filled up");
        return false;
    }

    arrayOfGuid.forEach(function (el) {
        var obj = {};
        obj.Guid = el;
        obj.Value = $("[data-id=" + el + "]").val();
        obj.FlexField = $("[data-id=" + el + "]").data('flexfield');
        formDataVMs.push(obj);
    })
    form.FormDataVMs = formDataVMs;
    form.FormTemplateId = $("#formTemplateId").val();
    $.ajax({
        type: "POST",
        url: "/Forms/Create",
        contentType: "application/json",
        data: JSON.stringify(form)
    }).done(function (response) {
        alert("success");
        $("#formBody").html('');
        $("#exampleModal").modal("hide");
    }).fail(function (XMLHttpReques, textStatus, errorThrown) {
        alert("Error occured in sending FormDatas");
    });
    $("#exampleModal").modal("hide");
})


function generateFormFieldHtml(formTemplateField) {
    switch (formTemplateField.ControlType) {
        case "Text":
            var strHtml = '<div class="form-group">' +
                '<label>' + formTemplateField.LabelFieldName + '</label >' +
                '<input type="text" data-flexfield="' + formTemplateField.FlexField + '" data-id="' + formTemplateField.Guid + '" class="form-control">' +
                '</div>';

            return strHtml;
            break;

        case "TextArea":
            var strHtml = '<div class="form-group">' +
                '<label>' + formTemplateField.LabelFieldName + '</label >' +
                '<textarea data-flexfield="' + formTemplateField.FlexField + '" data-id="' + formTemplateField.Guid + '" class="form-control"></textarea>' +
                '</div>';

            return strHtml;
            break;


        default:
    }
}


function checkEmptyArray(my_arr) {
    for (var i = 0; i < my_arr.length; i++) {
        if (my_arr[i] === "")
            return true;
    }
    return false;
}

$(function () {
    $('#exampleModal').on('hide.bs.modal', function (e) {
        $("#formBody").html('');
    });
});