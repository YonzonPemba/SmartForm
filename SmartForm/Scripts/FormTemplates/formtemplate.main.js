
$("#accordion").click(function (e) {
    var target = $(e.target);
    if (target.attr("id") == "accordion") {
        $(".nested li").toggle();
        $("#accordion").children('span').toggleClass("fa-caret-down fa-caret-up")
    }
})




$(document).on("click", ".nested li", function () {
    var guid = $(this).data('guid');

        $.ajax({
            type: "GET",
            url: "/FormTemplates/GetData",
            data: {guid:guid}
        }).done(function (response) {
            generateMarkup(JSON.parse(response));
            generateSetting(formTemplateFields[0]);

            if (!$("#formtemplate_delete_icon").length) {
                $(".formname").append('<i class="fa fa-trash icon" data-toggle="modal" data-target="#formTemplateModal" id="formtemplate_delete_icon" title="Delete FormTemplate"></i></span>');
            }
        }).fail(function (XMLHttpReques, textStatus, errorThrown) {
            alert("Error occured in receiving FormTemplateFields");
        });

})







function generateMarkup(datas) {

    $("#dropPlaceHolder").empty();
    formTemplateFields.length = 0;

    var formTemplateFieldsDB = datas.FormTemplateFields;

    formTemplateFieldsDB.forEach(function (el) {
        var string = addFormTemplateFields(el.ControlType, el);
        $("#dropPlaceHolder").append(string);
    })

    $("div.formControlWrapper.emptyDiv").droppable(droppableOptions);
    $("div.formControlWrapper.itemDiv").draggable(draggableOptions);

    $("#formTemplateName").val(datas.Name);
    $("#droppableHeader").text(datas.Name);
    $("#formtemplateguid").data('guid', datas.Guid);
}



function saveFormTemplate() {

    var formTemplateName = $("#formTemplateName").val();
    var formTemplate = {};

    if (formTemplateName.length == 0) {
        alert("FormTemplate name cannot be empty");
        return;
    }
    if (formTemplateFields.length == 0) {
        alert("No FormTemplate Field was created");
        return;
    }

    var guid = $("#formtemplateguid").data('guid');
    if (!guid) {
        formTemplate.Guid = generateUUID();
    } else {
        formTemplate.Guid = guid;
    }

    sortFormTemplateFields();
    formTemplate.Name = formTemplateName;
    formTemplate.FormTemplateFields = formTemplateFields;

    $.ajax({
        type: "POST",
        contentType: "application/json",
        url: '/FormTemplates/Create',
        data: JSON.stringify(formTemplate)
    }).done(function (response) {
        window.location.href = "/FormTemplates/Create"
    })
        .fail(function (XMLHttpReques, textStatus, errorThrown) {
            alert("error happened in ajax request");
        });


}

$(document).on("click", "#formtemplate_delete", function () {
    deleteFormTemplate();
    $("#formTemplateModal").modal("hide");
});

function deleteFormTemplate() {
    var guid = $("#formtemplateguid").data('guid');
    $.ajax({
        type: "POST",
        dataType:"text",
        url: '/FormTemplates/Delete',
        data: { guid: guid }
    }).done(function (response) {
        if (JSON.parse(response).Success) {
            window.location.href = "/FormTemplates/Create"
        }
        else {
            alert(response.Message);
        }
    })
        .fail(function (XMLHttpReques, textStatus, errorThrown) {
            alert("error happened in ajax request");
        });
}


function sortFormTemplateFields() {
    var guidAndPosition = $.map($('div.formControlWrapper.itemDiv'), function (el) {
        return {
            [$(el).children('div').data("guid")]: $(el).index()
        }
    });
    guidAndPosition.forEach(function (x) {

        for (let key in x) {

            index = formTemplateFields.findIndex(x => x.Guid === key);
            formTemplateFields[index].SortOrder = (x[key] + 1) / 2;

        }
    })
}