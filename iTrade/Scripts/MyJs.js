

function FormSubmit(formId) {
    var itemForm = $("#" + formId);//#EditInvDetInfoForm
    if (itemForm.length <= 0) {
        return false;
    }
    var ret = false;
    var url = itemForm.attr("action");
    var method = itemForm.attr("method");
    $.ajax({
        async: false,
        url: url,
        type: method,
        data: itemForm.serialize(),//itemForm.serializeArray(),
        success: function (result) {
            if (result.success) {
                ret = true;
            }
        }
    });

    return ret;
}

function ItemSave() {
    var ret = FormSubmit("EditInvDetInfoForm");
    if (ret == true) {
        var newUrl1 = $('#txtUrl1').val();
        $("#divOrderDetail").load(newUrl1);
    }
    return ret;
}

function MasterSave() {
    return FormSubmit("orderForm");
}
