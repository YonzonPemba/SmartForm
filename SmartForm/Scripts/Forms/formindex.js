var htmlString = "";

$("#filter").click(function () {
    $("#filterSection").toggle();
    $("#arrow").toggleClass("fa-caret-down fa-caret-up");
})

uniqueTemplate.forEach(function (x) {
    $("#FormTemplateName").append('<option value=' + x + '>' + x + '</option>');
})

if (data.length > 0) {
    console.log(data);
    generateHtml(data, uniqueTemplate);
}


$('#Status,#FormTemplateName').change(function () {

    $("#test").empty();
    htmlString = "";

    var status = $("#Status option:selected").text();
    var formTemplate = $("#FormTemplateName option:selected").text();
    var uniqueTemplatesArray = [];
    var filteredData;



    if (formTemplate == "All" && status == "All") {
        filteredData = data;
        uniqueTemplatesArray = uniqueTemplate;
    }
    else if (status == "All") {
        filteredData = data.filter(function (x) {
            return x.FormTemplateName == formTemplate;
        })
        uniqueTemplatesArray.push(formTemplate);
    }
    else if (formTemplate == "All") {
        filteredData = data.filter(function (x) {
            return x.Status == status;
        });
        var formTemplateNames = filteredData.map(function (x) { return x.FormTemplateName })
        uniqueTemplatesArray = new Set(formTemplateNames);
    }
    else {
        uniqueTemplatesArray.push(formTemplate);
        filteredData = data.filter(function (x) {
            return x.Status == status && x.FormTemplateName == formTemplate;
        })
    }

    if (filteredData.length > 0) {
        generateHtml(filteredData, uniqueTemplatesArray);
    }

});




function generateHtml(datas, uniqueTemplates) {

    htmlString += '<div class="card">' +
        '<div class="card-header extra">' +
        '<span class="card-header-text">Forms</span>' +
        '</div>' +
        '<div class="card-body">';

    //for (let value of uniqueTemplates) {

    //    particularTemplate = datas.filter(function (el) {
    //        return el.FormTemplateName == value;
    //    })

        //particularTemplate.forEach(function (x) {
        datas.forEach(function (x) {
            htmlString += '<div class="card mb-2">' +

                '<div class="card-header">' +
                '<span class="card-header-text">' + x.FormTemplateName + '</span><br>' +
                '</div>' +
                '<div class="card-body" onclick=Edit(' + x.FormId + ')>' +
                '<div class="row">';
            x.FormTemplateFields.forEach(function (ftf) {

                htmlString += '<div class="col-md-4"><label class="dark">' + ftf.LabelFieldName + '</label>' +
                    '<p>' + ftf.FlexFieldValue + '</p></div>';

            })
            htmlString += '</div></div>';
            htmlString += '<div class="card-footer d-flex justify-content-between">' +
                '<span class="card-header-text">' + x.Status + '</span><br>' +
                '<span class="card-header-text">' + x.CreatedBy + '</span>' +
                '</div>' +
                '</div>';
        })

    //}

    htmlString += '</div></div>';
    $("#test").append(htmlString);
}

function Edit(id) {
    window.location = '/Forms/Edit/' + id;
}