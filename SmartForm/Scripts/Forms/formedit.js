var htmlString = '';

htmlString += '<div class="card mt-3"><div class="card-header extra"><span class="card-header-text">' + datas.FormTemplateName + '<span></div><div class="card-body">';

datas.FormTemplateFields.forEach(function (el) {
    htmlString += generateFormFieldHtml(el);
})

    htmlString+=  '</div></div>';

$("#container").append(htmlString);

var array = datas.FormTemplateFields;
var textareas = array.filter(x => x.ControlType == "TextArea");

textareas.forEach(function (el) {
    $("[data-flexfield=" + el.FlexField + "]").val(el.FlexFieldValue);
})


function generateFormFieldHtml(formTemplateField) {
    switch (formTemplateField.ControlType) {
        case "Text":
            var strHtml = '<div class="form-group">' +
                '<label class="dark">' + formTemplateField.LabelFieldName + '</label >' +
                '<input type="text" data-flexfield="' + formTemplateField.FlexField + '" value="' + formTemplateField.FlexFieldValue + '" class="form-control">' +
                '</div>';

            return strHtml;
            break;

        case "TextArea":
            var strHtml = '<div class="form-group">' +
                '<label class="dark">' + formTemplateField.LabelFieldName + '</label >' +
                '<textarea data-flexfield="' + formTemplateField.FlexField + '" class="form-control"></textarea>' +
                '</div>';

            return strHtml;
            break;


        default:
    }
}


$(document).on("click", ".saveIcon", function () {

    datas.StatusId = $("#FormStatus").val()
    datas.FormTemplateFields.forEach(function (el) {
        el.FlexFieldValue = $("[data-flexfield=" + el.FlexField + "]").val();
    })

    $.ajax({
        type: "POST",
        url: "/Forms/Edit",
        contentType: "application/json",
        data: JSON.stringify(datas)
    }).done(function (response) {
        alert(response.Message);
        window.location = "/Forms/Index";

    }).fail(function (XMLHttpReques, textStatus, errorThrown) {
        alert("Error occured in updating FormDatas");
    });

})


