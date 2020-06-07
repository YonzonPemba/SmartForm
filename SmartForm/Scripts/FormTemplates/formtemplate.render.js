var draggableOptions = {
    axis: "y",
    revert: 'invalid',
    start: function (e, ui) {
        $(ui.helper).addClass('dragging');
    },
    stop: function (e, ui) {
        $(ui.helper).removeClass('dragging');
    }
}


var droppableOptions = {
    drop: function (event, ui) {
        if (!ui.draggable.hasClass('formControlWrapper')) {
            var $final = addFormTemplateFields(ui.draggable.data('controltype'));
            if (formTemplateFields.length == 1) {
                $("#dropPlaceHolder").empty();
                $("#dropPlaceHolder").append($final);
            }
            else {
                $(event.target).after($final);
            }

            $.each($final, function (index, value) {
                if ($(value).hasClass('emptyDiv')) {
                    $(value).droppable(droppableOptions);
                }
                else if ($(value).hasClass('itemDiv')) {
                    $(value).draggable(draggableOptions);
                }
            })
            var formTemplateField = formTemplateFields[formTemplateFields.length - 1];
            generateSetting(formTemplateField);
        }
        else {
            var $itemDiv = $(ui.draggable);
            var $emptyDiv = $(ui.draggable).next();

            $(ui.draggable).css('top', 0);
            if ($(ui.draggable).next().is($(event.target)) || $(ui.draggable).prev().is($(event.target))) {
                return;
            }

            $(event.target).after($emptyDiv);
            $(event.target).after($itemDiv);
        }
    },
    over: function (event, ui) {
        ui.helper.css('z-index', 1);
    }

}

var formTemplateFields = [];

function addFormTemplateFields(controlType, formField = "") {
    var guid;
    var formTemplateField = {};

    if (formField == "") {
        guid = generateUUID();
        formTemplateField.Guid = guid;
        formTemplateField.SortOrder = 0;
        formTemplateField.ControlType = controlType;
        formTemplateField.LabelFieldName = controlType;
        formTemplateField.FlexField = "";
        formTemplateField.FlexFieldValue = "";
    }
    else {
        formTemplateField = formField;
    }
    formTemplateFields.push(formTemplateField);

    var strHtml = generateFormFieldHtml(formTemplateField);
    var $control = addControlWrapper("itemDiv").append($(strHtml));
    var $final;
    if (formTemplateFields.length == 1) {
        $final = addControlWrapper("emptyDiv").append(addEmptyDiv()).add($control).add(addControlWrapper("emptyDiv").append(addEmptyDiv()));
    }
    else {
         $final = $control.add(addControlWrapper("emptyDiv").append(addEmptyDiv()));
    }

    return $final;
}


function generateSetting(formTemplateField) {

    $("#setting").empty();
    $("#dropPlaceHolder>div.highlight").removeClass('highlight');
    $("#labelfield").empty();
    $("#labelfield").append(getIcon(formTemplateField.ControlType) + '<p class="mb-0">Control Property</p>');
    $("#labelfield").append('<i title="Delete FormTemplateField" class="fa fa-trash icon btnDelete" guid="' + formTemplateField.Guid + '" data-toggle="modal" data-target="#exampleModal"></i>')


    var stringHtml = '<label class="card-header-text">LabelField</label><input type="text" class="form-control" id="settingGuid" guid="' + formTemplateField.Guid + '" value="' + formTemplateField.LabelFieldName + '" />';

    $("#setting").append(stringHtml);
    $("label[guid=" + formTemplateField.Guid + "]").parents(".formControlWrapper").addClass("highlight");

}

function addControlWrapper(type) {

    var $wrapper = $('<div>')
        .addClass('formControlWrapper');

    if (type == "emptyDiv") {
        $wrapper.addClass('emptyDiv');
    }
    if (type == "itemDiv") {
        $wrapper.addClass('itemDiv');
    }

    return $wrapper;
}


function getIcon(controlType) {
    switch (controlType) {
        case "Text":
            var strHtml = '<span class="material-icons icon">text_fields</span>';
            return strHtml;
            break;

        case "TextArea":
            var strHtml = '<span class="material-icons icon">text_format</span>';
            return strHtml;
            break;


        default:
    }
}

function addEmptyDiv() {
    var $wrap = $('<div>').addClass('droppable');
    return $wrap;
}

function generateFormFieldHtml(formTemplateField) {
    switch (formTemplateField.ControlType) {
        case "Text":
            var strHtml = '<div class="text" data-guid="' + formTemplateField.Guid + '" onclick = "fieldClicked(this)">' +
                '<label guid = "' + formTemplateField.Guid + '">' + formTemplateField.LabelFieldName + '</label >' +
                '<input type="text" />' +
                '</div>';

            return strHtml;
            break;

        case "TextArea":
            var strHtml = '<div class="text-area" data-guid="' + formTemplateField.Guid + '" onclick = "fieldClicked(this)">' +
                '<label guid = "' + formTemplateField.Guid + '">' +formTemplateField.LabelFieldName+'</label >' +
                '<textarea></textarea>' +
                '</div>';

            return strHtml;
            break;


        default:
    }
}

function generateUUID() {
    var d = new Date().getTime();
    var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = (d + Math.random() * 16) % 16 | 0;
        d = Math.floor(d / 16);
        return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
    return uuid;
}


function fieldClicked(el) {
    var guid = $(el).data('guid');
    var formTemplateField = formTemplateFields.filter(function (el) {
        return el.Guid == guid;
    })

    generateSetting(formTemplateField[0]);
}



$(document).on("blur", "#settingGuid", function () {

    var guid = $("#settingGuid").attr('guid');

    if (guid) {
        var formTemplateField = formTemplateFields.filter(function (el) {
            return el.Guid == guid;
        })
        var texts = $("#settingGuid").val();
        if (texts.length == 0) {
            $("#settingGuid").val(formTemplateField[0].LabelFieldName);
            return;
        }
        var index = formTemplateFields.findIndex(function (item, i) {
            return item.Guid == guid;
        });
        if (index >= 0) {
            formTemplateFields[index].LabelFieldName = texts;
        }
        $("label[guid=" + guid + "]").html(formTemplateFields[index].LabelFieldName);

    }
})

$(document).on("click", "#formtemplatefield_delete", function () {

    var guid = $("#settingGuid").attr('guid');
    if (guid) {
        formTemplateFields = formTemplateFields.filter(function (el) {
            return el.Guid != guid;
        })

        highlightNextField(guid);

        $("label[guid=" + guid + "]").parents(".formControlWrapper").next().remove();
        $("label[guid=" + guid + "]").parents(".formControlWrapper").remove();

        

        if (formTemplateFields.length==0) {
            $('div.formControlWrapper.emptyDiv').addClass('h-100');
            $('div.formControlWrapper.emptyDiv').children().addClass('h-100').addClass('initial');
            $("#setting").empty();
            $("#labelfield").empty();
            $("#labelfield").append('<p class="mb-0">Control Property</p>');
        }
    }

    $("#exampleModal").modal("hide");
});


function highlightNextField(guid) {
    var guidAndPosition = $.map($('div.formControlWrapper.itemDiv'), function (el) {
        return {
            [$(el).children('div').data("guid")]: $(el).index()
        }
    });

    var position;
    guidAndPosition.forEach(function (x, index) {
        var keys = Object.keys(x);
        if (keys.includes(guid)) {
            position = index;
        }
    })

    var nextGuid;
    if (guidAndPosition.length > 1) {
        if (position == guidAndPosition.length - 1) {
            nextGuid = guidAndPosition[guidAndPosition.length - 2];
        }
        else {
            nextGuid = guidAndPosition[position + 1];
        }
        var nextFormTemplateField = formTemplateFields.filter(function (x) {
            return x.Guid == Object.keys(nextGuid)[0];
        })
        generateSetting(nextFormTemplateField[0]);
    }
}


$(".draggable").dblclick(function () {
    var $strHtml = addFormTemplateFields($(this)[0].childNodes[1].nodeValue);
    if (formTemplateFields.length == 1) {
        $("#dropPlaceHolder").empty();
    }
    $("#dropPlaceHolder").append($strHtml);
    $.each($strHtml, function (index, value) {

        if ($(value).hasClass('emptyDiv')) {
            $(value).droppable(droppableOptions);
        }
        else if ($(value).hasClass('itemDiv')) {
            $(value).draggable(draggableOptions);
        }
    })
    var formTemplateField = formTemplateFields[formTemplateFields.length - 1];
    generateSetting(formTemplateField);

})






$(document).on("blur", "#formTemplateName", function () {

    $("#droppableHeader").text($("#formTemplateName").val());
})

$(".nested li").click(function () {
    $(".nested li").removeClass("active");
    $(this).addClass("active");
})





