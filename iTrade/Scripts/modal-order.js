$(function () {
    $.ajaxSetup({ cache: false });
    $(document).on("click", "a[data-modal]", function () {
        $('#myModalContent').load(this.href, function () {
            $('#myModal').modal({
                keyboard: true
            }, 'show');
            bindForm(this);
        });
        return false;
    });


});

function bindForm(dialog) {
    $('form', dialog).submit(function () {
        $('#progress').show();
        var newUrl1 = $('#txtUrl1').val();
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (result) {
                if (result.success) {
                    $('#myModal').modal('hide');
                    $('#progress').hide();

                    $("#divOrderDetail").load(newUrl1);

                    //location.reload();


                } else {
                    $('#progress').hide();
                    $('#myModalContent').html(result);
                    bindForm();
                }
            }
        });
        return false;
    });
}


$('#btnTransfer').click(function () {

    if ($("#txtQty").val().length > 0) {
        var newDate = $('#txtDate').val();
        var newVariantID = $('#txtVariantID').val();
        var newQty = $('#txtQty').val();
        var newFromLocationID = $('#txtFromLocationID').val();
        var newToLocationID = $('#txtToLocationID').val();
         

    //    alert(newDate + '**' + newVariantID + '**' + newQty + '**' + newFromLocationID + '**' + newToLocationID );

        $.ajax({
            type: 'GET',
            url: '/Inventory/_Transfer',
            dataType: "json",
            contentType: 'application/json, charset=utf-8',
            data: {
                valDate: newDate, valVariantID: newVariantID, valQty: newQty, valFromLocationID: newFromLocationID, valToLocationID: newToLocationID
            },
            success: function (result) {

                if (result.success) {
 
                       alert("Successful Transferred.");
                  //  location.reload();
                } else {
                  
                   // $('#discountModalContent').html(result);
                       alert("Sorry not able to transfer.");
                }


            },
            error: function (xhr, status, error) {
                alert(error);
            }

        });

    }
    else {
        alert("Not valid input.");
    };


});



//$('#editKivQtyForm').submit(function (e) {
//    e.preventDefault();
//    alert("hiaaa...");
//    var list = JSON.stringify('list', $(this).serializeArray());

//    $.ajax({
//        traditional: true,
//        contentType: 'application/json; charset=utf-8',
//        dataType: 'json',
//        type: 'POST',
//        url: this.action,
//        data: list,
//        success: function () {
//            alert("KIV Saved.");
//        },
//        failure: function (response) {
//            alert("KIV not saved.");
//            //  $('#result').html(response);
//        }
//    });
//});



$('#kivOrderForm').submit(function (e) {
    e.preventDefault();
    var kivid = $("#korNumber").val().trim();
    var newUrl1 = $('#txtUrl1').val();
    var newUrl2 = $('#txtUrl2').val();
    var btnAction = $("#actionType").val();
    $.ajax({
        url: this.action,
        type: "POST",
        data: $(this).serialize(),
        success: function (result) {
            if (result.success) {
                if (btnAction == "SaveAndStay") {
                    //  alert("Data Saved.");
                    window.location.reload(true);

                }

                if (btnAction == "SaveAndSubmit") {
                    //  alert("submitting..");
                    KivOrderSubmit(kivid);
                }

                if (btnAction == "SaveAndAdd" && result.redirectUrl != null) {

                    window.location = result.redirectUrl;

                }
            }
        }
    });
});


$('#orderForm').submit(function (e) {
    e.preventDefault();
    var sorid = $("#sorNumber").val().trim();
    var invtype = $("#invType").val().trim();
    var btnAction = $("#actionType").val();
    $.ajax({
        url: this.action,
        type: "POST",
        data: $(this).serialize(),
        success: function (result) {
            if (result.success) {
                if (btnAction == "SaveAndStay") {
                  //  SaveInvDets();
                  //  SaveKivDets();
                    //  alert("Data Saved.");
                    window.location.reload();
                }

                if (btnAction == "SaveAndSubmit") {
                    if (invtype == "PRO") {
                        ConfirmSubmitPRO(sorid);
                    } else {
                        ConfirmSubmit(sorid);
                    }

                }

                if (btnAction == "SaveAndAdd" && result.redirectUrl != null) {
            
                        window.location = result.redirectUrl;                

                } 
            } 
        }
    });
});


function SaveInvDets() {
 
    $("#btnSubmitInvDetInfo").trigger("click");
}
function SaveKivDets() {

    $("#btnSubmitKivInfo").trigger("click");
}


function ConfirmSubmit(sorno) {
    var bChecked = true;
    if (bChecked) {
        $.ajax({
            type: 'GET',
            url: '/Orders/_SubmitSalesOrder',
            dataType: "json",
            contentType: 'application/json, charset=utf-8',
            data: {
                SorID: sorno
            },
            success: function (json) {
                if (json.success != null && !json.success) {
                    alert(json.responseText);
                }
                if (json.isRedirect) {
                    $('#headerBefore').removeClass();
                    $('#headerBefore').addClass("hide");
                    $('#headerAfter').removeClass();
                    $('#headerAfter').addClass("show");
                    $('#paymentBox').removeClass();
                    $('#paymentBox').addClass("hide");
                    $('#paymentMessage').removeClass();
                    $('#paymentMessage').addClass("Show");
                    $("#btnPrint").attr("href", json.printUrl);
                    $("#btnPrint2").attr("href", json.printUrl);
                    $('#btnPrint').trigger('click');

                }

            },
            error: function (xhr, status, error) {
                alert(error);
            }

        });

    }
    else {
        alert("Data not valid.");
    };
}


function ConfirmSubmitPRO(sorno) {
    var bChecked = true;
    if (bChecked) {
        $.ajax({
            type: 'GET',
            url: '/Orders/_SubmitProjectOrder',
            dataType: "json",
            contentType: 'application/json, charset=utf-8',
            data: {
                SorID: sorno
            },
            success: function (json) {
                if (json.success != null && !json.success) {
                    alert(json.responseText);
                }
                if (json.isRedirect) {
                    $('#headerBefore').removeClass();
                    $('#headerBefore').addClass("hide");
                    $('#headerAfter').removeClass();
                    $('#headerAfter').addClass("show");
                    $('#paymentBox').removeClass();
                    $('#paymentBox').addClass("hide");
                    $('#paymentMessage').removeClass();
                    $('#paymentMessage').addClass("Show");
                    $("#btnPrint").attr("href", json.printUrl);
                    $("#btnPrint2").attr("href", json.printUrl);
                    $('#btnPrint').trigger('click');

                }

            },
            error: function (xhr, status, error) {
                alert(error);
            }

        });

    }
    else {
        alert("Data not valid.");
    };
}

function KivOrderSubmit(sorno) {
    var bChecked = true;
    if (bChecked) {
        $.ajax({
            type: 'GET',
            url: '/KivOrders/_SubmitKivOrderPreview',
            dataType: "json",
            contentType: 'application/json, charset=utf-8',
            data: {
                SorID: sorno
            },
            success: function (json) {
                if (json.success != null && !json.success) {
                    alert(json.responseText);
                }
                else {

                    $('#contentBefore').removeClass();
                    $('#contentBefore').addClass("hide");
                    $('#contentAfter').removeClass();
                    $('#contentAfter').addClass("show");
                    $('#headerBefore').removeClass();
                    $('#headerBefore').addClass("hide");
                    $('#headerAfter').removeClass();
                    $('#headerAfter').addClass("show");
                    $("#btnPrint").attr("href", json.printUrl);
                    $("#btnPrint2").attr("href", json.printUrl);
                    $('#btnPrint').trigger('click');
                }
                if (json.isRedirect) {

                }

            },
            error: function (xhr, status, error) {
                alert(error);
            }

        });

    }
    else {
        alert("Data not valid.");
    };
}


$('#addItemForm').submit(function (e) {
    e.preventDefault();

    var dettype = $('.rDetType').filter(':checked').val().trim();
    if (dettype == "PRODUCT") {
        if (($('#itemName').val() == "") || ($('#itemID').val() == 0)) {
            alert("Please input product");
            return false;
        };
    } else {
        var itemdesc = $('#itemDesc').val().trim();
        $('#itemName').val(itemdesc);
        $('#itemCode').val("*OT");
    }

    //if ($('#itemQty').val() < "0") {
    //    alert("Please provide quantity");
    //    return false;
    //};
    //if ($('#itemDiscountedPrice').val() < "0") {
    //    alert("Sell price can not less than 0");
    //    return false;
    //};
    var itemname = $('#itemName').val();
    var newUrl1 = $('#txtUrl1').val();
    var newUrl2 = $('#txtUrl2').val();
    var btnAction = $("#btnActionType").val();

    $.ajax({
        url: this.action,
        type: "POST",
        data: $(this).serialize(),
        success: function (result) {
            $("#Div").html(result);
            if (result.success) {
                $('#txtSearch').val("");
                $('#itemName').val("");
                $('#itemCode').val("");
                $('#itemType').val("");
                $('#itemDesc').val("");
                $('#itemUnit').val("");
                $('#itemCostCode').val("");
                $('#itemListPrice').val(0);
                $('#itemShowroomPrice').val(0);
                $('#itemSpecialNett').val(0);

                $('#itemQty').val(0);
                $('#itemUnitPrice').val(0);
                $('#itemDiscountValue').val(0);
              //  $('#itemDiscount').val(0);

                $('#itemPreDiscTotal').val(0);
                $('#itemDiscountPercentage').val(0);
                $('#itemDiscountTotal').val(0);

                $('#itemDiscountedPrice').val(0);
                $('#itemAmount').val(0);
                $('#itemRemark').val("");
                $('#txtOnHand').text("");

                var tot = result.totalamount.toFixed(2);

                $('#txtTotalAmount').text(tot);
                $('#txtLastItem').text("Last item: " + result.detcount + ") " + itemname);

                $('#optionPricebreak').empty();
                $('#divLast3Prices').empty();
                $('#divLast3PricesOthers').empty();

                $('#txtSearch').focus();
                $('#txtSearch').select();

                $("#divOrderDetail").load(newUrl1);

                $("#btnAddItem").prop('disabled', false);
                $("#btnAddItem2").prop('disabled', false);

                if (btnAction == "SaveAndClose") {
                    $("#additemModal").modal('hide');
                }

                //$("#divKivDets").load(newUrl2);
            }

            //if (result.redirectUrl != null) {
            //    window.location = result.redirectUrl;
            //}
        }

    });
});

$('#addDetForm').submit(function (e) {
    e.preventDefault();

    if ($('#txtSearch').val() == "") {
        alert("Please input product");
        return false;
    };
    if ($('#itemQty').val() < "0") {
        alert("Please provide quantity");
        return false;
    };
    if ($('#itemDiscountedPrice').val() < "0") {
        alert("Sell price can not less than 0");
        return false;
    };

    $.ajax({
        url: this.action,
        type: "POST",
        data: $(this).serialize(),
        success: function (result) {
            $("#Div").html(result);
            //if (result.success) {
            //    $('#itemQty').val(0);
            //    $('#itemDiscountedPrice').val(0);

            //}

            if (result.redirectUrl != null) {
                window.location = result.redirectUrl;
            }
        }

    });
});


$('#addMultiItemForm').submit(function (e) {
    e.preventDefault();

    var newUrl1 = $('#txtUrl1').val();
    var url = $('#txtUrl3').val();
    var btnAction = $("#btnActionType3").val();
    var sorID = $('#valSorID').val();

    $.ajax({
        url: this.action,
        type: "POST",
        data: $(this).serialize() + 'valSorID = ' + sorID,
        success: function (result) {
            $("#Div").html(result);
            if (result.success) {
                $('#addMultiitemModal #txtSearchKeyword').val("");
                $('#addMultiitemModal').modal('show');
                $('#txtSearchKeyword').focus();
                $('#txtSearchKeyword').select();

                $("#divSearchResultList").load(url);
                $("#divOrderDetail").load(newUrl1);

                $("#btnAddItem3a").prop('disabled', false);
                $("#btnAddItem3b").prop('disabled', false);

                if (btnAction == "SaveAndClose") {
                    $("#addMultiitemModal").modal('hide');
                }

                //$("#divKivDets").load(newUrl2);
            }

            //if (result.redirectUrl != null) {
            //    window.location = result.redirectUrl;
            //}
        }

    });
});


//$('#editItemListForm').submit(function (e) {
//    e.preventDefault();

//    var newUrl1 = $('#txtUrl1').val();
//    var sorID = $('#valSorID').val();

//    $.ajax({
//        url: this.action,
//        type: "POST",
//        data: $(this).serialize(),
//        success: function (result) {
//            $("#Div").html(result);
//            if (result.success) {

//                $("#divOrderDetail").load(newUrl1);

//                //$("#divKivDets").load(newUrl2);
//            }

//            //if (result.redirectUrl != null) {
//            //    window.location = result.redirectUrl;
//            //}
//        }

//    });
//});



$("#addBundelProductForm").submit(function () {
    $('#progress').show();
    $.ajax({
        url: this.action,
        type: this.method,
        data: $(this).serialize(),
        success: function (result) {
            if (result.success) {
                $('#bundleOrderModal').modal('hide');
                $('#progress').hide();
                location.reload();
            } else {
                $('#progress').hide();
                //  $('#bundleOrderModalContent').html(result);
            }
        }
    });
    return false;
});





var decSeparator = ".";//or ","
(function (jQuery) {
    function internalCheck(code, ch, key, v) {
        if (key == code)
            if (ch == decSeparator) {
                if (v.indexOf(decSeparator) != -1)
                    return false;
            } else {
                return false;
            }
        return true;
    }

    jQuery.fn.forceNumeric = function (options) {
        var opts = jQuery.extend({}, jQuery.fn.forceNumeric.defaults, options);

        return this.each(function () {
            var o = jQuery.meta ? jQuery.extend({}, opts, $this.data()) : opts;
            $(this).keydown(function (e) {
                var key = e.which || e.keyCode;

                if (!e.shiftKey && !e.altKey && !e.ctrlKey &&
                    // numbers   
                    key >= 48 && key <= 57 ||
                    // Numeric keypad
                    key >= 96 && key <= 105 ||
                    // comma, period and minus, . on keypad      '
                   key == 190 || key == 188 || key == 109 || key == 110 || key == 222 ||
                    // Backspace and Tab and Enter
                   key == 8 || key == 9 || key == 13 ||
                    // Home and End
                   key == 35 || key == 36 ||
                    // left and right arrows
                   key == 37 || key == 39 ||
                    // Del and Ins
                   key == 46 || key == 45) {
                    var v = $(this).val();

                    var tmp = internalCheck(190, ".", key, v);
                    if (!tmp) return false;

                    var tmp = internalCheck(188, ",", key, v);
                    if (!tmp) return false;

                    return true;
                } else if (e.ctrlKey) {
                    //ctrl-c       ctrl-v       ctrl-x
                    if (key == 67 || key == 86 || key == 90)
                        return true;
                }
                return false;
            });

            $(this).blur(function (e) {
                var v = jQuery.trim($(this).val());
                if (v == '') {
                    return;
                }

                if (o.fixDecimals != -1) {
                    var num = parseFloat(v);
                    var numSix = num.toFixed(o.fixDecimals);
                    $(this).val(numSix);
                }

            });


        });
        jQuery.fn.forceNumeric.defaults = {
            fixDecimals: -1
        };
    };
})(jQuery);


$('#btnConvert').click(function () {
    if ($("#txtSorID").val().length > 0) {
        var SorID = $('#txtSorID').val();

        // alert(SorID);

        $.ajax({
            type: 'GET',
            url: '/Orders/_ConvertToInvoice',
            dataType: "json",
            contentType: 'application/json, charset=utf-8',
            data: {
                valSorID: SorID
            },
            success: function (json) {
                if (json.isRedirect) {
                    window.location.href = json.redirectUrl;
                }

            },
            error: function (xhr, status, error) {
                alert(error);
            }

        });

    }
    else {
        alert("Not valid input.");
    };


});


$('#btnUpdateStockTakeDets').click(function () {
    var newProductID = $('#txtProductID').val();
    var newStockTakeQty = $('#txtStockTakeQty').val();
    var newSttID = $('#txtSttID').val();
    var newLocationID = $('#txtLocationID').val();
  //  alert("hi " + newProductID + "| " + newStockTakeQty + "| " + newSttID + "| " + newLocationID);

    if ($("#txtStockTakeQty").val().length > 0) {


        var newUrl = $('#txtUrl').val();

        // alert(InvID + '**' + newDiscount + '**' + newAmount + '**' + newGst + '**' + newNett + '**' + newUrl);

        $.ajax({
            type: 'GET',
            url: '/Inventory/_StockTakeSave',
            dataType: "json",
            contentType: 'application/json, charset=utf-8',
            data: {
                valProductID: newProductID, valStockTakeQty: newStockTakeQty, valSttID: newSttID, valLocationID: newLocationID
            },
            success: function (result) {

                if (result.success) {
                    $('#stocktakeModal').modal('show');
                    $('#progress').hide();
                    //  alert("Successful Saved.");
                    $('#txtSearch').val("");
                    $('#txtProductID').val(0);
                    $('#txtSKU').text("");
                    $('#txtProductType').text("");
                    $('#txtProductName').text("");
                    $('#txtInStock').text("");
                    $('#txtAllocated').text("");
                    $('#txtOnHand').text("");
                    $('#txtStockTakeQty').val("");

                    $('#txtSearch').focus();
                    $('#txtSearch').select();

                 //   $("#sessionStockTakeResults").load(newUrl);

                } else {
                    $('#progress').hide();
                    $('#sessionStockTakeResults').html(result);
                    //   alert("hello 2");
                }


            },
            error: function (xhr, status, error) {
                alert(error);
            }

        });

    }
    else {
        alert("Not valid input.");
    };


});


$('#additemModal').on('shown.bs.modal', function () {
    $('#txtSearch').focus();
    $('#txtSearch').select();

});

$('#MultiPrintModal').on('shown.bs.modal', function () {

    var invAmount = $('#divOrderDetail #invNett').val();
    var invOutstanding = $('#divOrderDetail #invOutstanding').val();
    $('#invAmountText').text(invAmount);
    $('#invOutstandingText').text(invOutstanding);
    //$('#chkCashAmount').val(invOutstanding);

    // Uncheck checkbox
    $('#chkWithPayment').prop('checked', false);
    $('#btnSubmitOrderAndInvoice').prop('disabled', false);

    $('#chkCash').prop('checked', false);
    $('#chkNETS').prop('checked', false); // Unchecks it
    $('#chkCreditCard').prop('checked', false); // Unchecks it
    $('#chkCheque').prop('checked', false); // Unchecks it
    $('#chkCreditNote').prop('checked', false); // Unchecks it

    $('#chkCash').prop('disabled', true); // Disable checkbox
    $('#chkNETS').prop('disabled', true);
    $('#chkCreditCard').prop('disabled', true);
    $('#chkCheque').prop('disabled', true);
    $('#chkCreditNote').prop('disabled', true);

    $('#chkCashAmount').val(0); // Reset textbox to 0
    $('#chkNETSAmount').val(0);
    $('#chkCreditCardAmount').val(0);
    $('#chkChequeAmount').val(0);

    $('#chkCashAmount').prop('readonly', true);
    $('#chkNETSAmount').prop('readonly', true);
    $('#chkCreditCardAmount').prop('readonly', true);
    $('#chkChequeAmount').prop('readonly', true);
    $('#chkCreditNoteAmount').prop('readonly', true);

    $('#chkNETSNumber').val("");
    $('#chkCreditCardNumber').val("");
    $('#chkChequeNumber').val("");
    $('#chkCreditNoteNumber').val("");

    $('#chkNETSNumber').prop('readonly', true);
    $('#chkCreditCardNumber').prop('readonly', true);
    $('#chkChequeNumber').prop('readonly', true);
    $('#chkCreditNoteNumber').prop('readonly', true);



});

$('#MultiPrintModal').on('hidden.bs.modal', function () {

    //$('#chkCashAmount').val(0);
    //$('#chkNETSAmount').val(0);
    //$('#chkCreditCardAmount').val(0);
    //$('#chkChequeAmount').val(0);

    //$('#chkChequeNumber').val("");

    //$('#chkCashAmount').prop('readonly', true);
    //$('#chkNETSAmount').prop('readonly', true);
    //$('#chkCreditCardAmount').prop('readonly', true);
    //$('#chkChequeAmount').prop('readonly', true);
    //$('#chkChequeNumber').prop('readonly', true);

    //$('#chkWithoutPayment').prop('checked', true); // checks it
    //$('#chkCash').prop('checked', false); // Unchecks it
    //$('#chkNETS').prop('checked', false); // Unchecks it
    //$('#chkCreditCard').prop('checked', false); // Unchecks it
    //$('#chkCheque').prop('checked', false); // Unchecks it

    //$('#chkCash').prop('disabled', false); // Disable it
    //$('#chkNETS').prop('disabled', false);
    //$('#chkCreditCard').prop('disabled', false);
    //$('#chkCheque').prop('disabled', false);

});

$('#QuotationModal').on('shown.bs.modal', function () {

    $('#chkWithoutPayment').prop('checked', true);

    var invAmount = $('#divOrderDetail #invNett').val();
    $('#invAmountText').text(invAmount);
    $('#chkCashAmount').val(0);
    $('#chkCashAmount').prop('readonly', true);
    $('#chkCash').prop('checked', false);
    $('#chkCash').prop('disabled', true);



});

$('#QuotationModal').on('hidden.bs.modal', function () {

    $('#chkCashAmount').val(0);
    $('#chkNETSAmount').val(0);
    $('#chkCreditCardAmount').val(0);
    $('#chkChequeAmount').val(0);
    $('#chkChequeNumber').val("");

    $('#chkCashAmount').prop('readonly', true);
    $('#chkNETSAmount').prop('readonly', true);
    $('#chkCreditCardAmount').prop('readonly', true);
    $('#chkChequeAmount').prop('readonly', true);
    $('#chkChequeNumber').prop('readonly', true);

    $('#chkWithoutPayment').prop('checked', false); // Unchecks it
    $('#chkCash').prop('checked', false); // Unchecks it
    $('#chkNETS').prop('checked', false); // Unchecks it
    $('#chkCreditCard').prop('checked', false); // Unchecks it
    $('#chkCheque').prop('checked', false); // Unchecks it

    $('#chkCash').prop('disabled', true); // Disable it
    $('#chkNETS').prop('disabled', true);
    $('#chkCreditCard').prop('disabled', true);
    $('#chkCheque').prop('disabled', true);

});

$('#TakePaymentModal').on('shown.bs.modal', function () {
    var tot = $("#invOutstanding").val();
    if (tot <= 0) {
        $('#TakePaymentModal').modal('hide');
        alert("Please input pay amount.");
        return false;
    }

    var invAmount = $('#divOrderDetail #invNett').val();
    var invOutstanding = $('#divOrderDetail #invOutstanding').val();
    $('#invAmountText').text(invAmount);
    $('#invOutstandingText').text(invOutstanding);
    $('#chkCashAmount').val(invOutstanding);

    // Uncheck checkbox
    $('#chkWithPayment').prop('checked', false);
    $('#btnSubmitOrderAndInvoice').prop('disabled', false);

    $('#chkCash').prop('checked', false);
    $('#chkNETS').prop('checked', false); // Unchecks it
    $('#chkCreditCard').prop('checked', false); // Unchecks it
    $('#chkCheque').prop('checked', false); // Unchecks it
    $('#chkCreditNote').prop('checked', false); // Unchecks it

    $('#chkCash').prop('disabled', true); // Disable checkbox
    $('#chkNETS').prop('disabled', true);
    $('#chkCreditCard').prop('disabled', true);
    $('#chkCheque').prop('disabled', true);
    $('#chkCreditNote').prop('disabled', true);

    $('#chkCashAmount').val(0); // Reset textbox to 0
    $('#chkNETSAmount').val(0);
    $('#chkCreditCardAmount').val(0);
    $('#chkChequeAmount').val(0);

    $('#chkCashAmount').prop('readonly', true);
    $('#chkNETSAmount').prop('readonly', true);
    $('#chkCreditCardAmount').prop('readonly', true);
    $('#chkChequeAmount').prop('readonly', true);
    $('#chkCreditNoteAmount').prop('readonly', true);

    $('#chkNETSNumber').val("");
    $('#chkCreditCardNumber').val("");
    $('#chkChequeNumber').val("");
    $('#chkCreditNoteNumber').val("");

    $('#chkNETSNumber').prop('readonly', true);
    $('#chkCreditCardNumber').prop('readonly', true);
    $('#chkChequeNumber').prop('readonly', true);
    $('#chkCreditNoteNumber').prop('readonly', true);



});


$('#addPricebookForm').submit(function (e) {
    e.preventDefault();
    var newUrl1 = $('#txtUrl1').val();

    $.ajax({
        url: this.action,
        type: "POST",
        data: $(this).serialize(),
        success: function (result) {
            if (result.success) {
                $("#divItemList").load(newUrl1);
                alert("Item Added.");
            }
        }
    });
});

$('#addEnrolmentForm').submit(function (e) {
    e.preventDefault();
    var newUrl1 = $('#txtUrl1').val();

    $.ajax({
        url: this.action,
        type: "POST",
        data: $(this).serialize(),
        success: function (result) {
            if (result.success) {
                $("#divItemList").load(newUrl1);
                //  alert("Item Added.");
                $("#MultiPrintModal").modal('show');
                $('#headerBefore').removeClass();
                $('#headerBefore').addClass("hide");
                $('#headerAfter').removeClass();
                $('#headerAfter').addClass("show");
                $('#contentBefore').removeClass();
                $('#contentBefore').addClass("hide");
                $('#contentAfter').removeClass();
                $('#contentAfter').addClass("show");

                $("#btnInvoice").attr("href", result.invUrl);
                $('#btnInvoice').trigger('click');

                $("#btnPay").attr("href", result.paymentUrl);
                $('#btnPay').trigger('click');

            }
        }
    });
});

$('#editEnrolmentForm').submit(function (e) {
    e.preventDefault();
    var newUrl1 = $('#txtUrl1').val();

    $.ajax({
        url: this.action,
        type: "POST",
        data: $(this).serialize(),
        success: function (result) {
            if (result.success) {
                $("#divItemList").load(newUrl1);
                alert("Data saved success.");

            }
        }
    });
});



$('#CourseOrderForm').submit(function (e) {
    e.preventDefault();
    var sorid = $("#sorNumber").val().trim();
    var btnAction = $("#actionType").val();
    $.ajax({
        url: this.action,
        type: "POST",
        data: $(this).serialize(),
        success: function (result) {
            if (result.success) {
                if (btnAction == "SaveAndStay") {
                    //  SaveInvDets();
                    //  SaveKivDets();
                    alert("Data Saved.");
                }

                if (btnAction == "SaveAndSubmit") {
                    ConfirmSubmitCourseOrder(sorid);
                }

                if (btnAction == "SaveAndAdd" && result.redirectUrl != null) {

                    window.location = result.redirectUrl;

                }
            }
        }
    });
});

$('#batchBillingForm').submit(function (e) {
    e.preventDefault();

    var newUrl1 = $('#txtUrl1').val();
    var url = $('#txtUrl3').val();
    var btnAction = $("#btnActionType3").val();
    var sorID = $('#valSorID').val();
    var month = $('#valMonth').val();

    $.ajax({
        url: this.action,
        type: "POST",
        data: $(this).serialize() + 'valSorID = ' + sorID + 'valMonth = ' + month,
        success: function (result) {
            $("#Div").html(result);
            if (result.success) {
                // alert("hi...done");

                $('#MultiPrintModal').modal('show');
                $("#btnInvoice").attr("href", result.redirectUrl);
                $('#btnInvoice').trigger('click');

                $('#btnBatchInvoice').prop('disabled', false);

            }

            //if (result.redirectUrl != null) {
            //    window.location = result.redirectUrl;
            //}
        }

    });
});

$('#addScheduleForm').submit(function (e) {
    e.preventDefault();
    //   var sorid = $("#sorNumber").val().trim();

    if ($('#priceID2').val() == "0") {
        alert("Please select course.");
        return false;
    };

    if ($('#fromDate').val() == "") {
        alert("Please select valid from date.");
        return false;
    };

    var btnAction = $("#btnActionType").val();
    $.ajax({
        url: this.action,
        type: "POST",
        data: $(this).serialize(),
        success: function (result) {
            if (result.success) {
                if (btnAction == "SaveAndNext") {

                    alert("Data Saved.");
                }

                if (btnAction == "SaveAndClose") {
                    $("#additemModal").modal('hide');

                    if (result.redirectUrl != null) {
                        window.location = result.redirectUrl;
                    }

                }
            }
        }
    });
});

$('#addScheduleByDateForm').submit(function (e) {
    e.preventDefault();
    //   var sorid = $("#sorNumber").val().trim();

    var date1 = $("#Date1").val().trim();
    var date2 = $("#Date2").val().trim();

    if ($('#priceID3').val() == "0") {
        alert("Please select course.");
        return false;
    };
    if ($('#Date1').val() == "") {
        alert("Please select from date.");
        return false;
    };
    if ($('#Date2').val() == "") {
        alert("Please select to date.");
        return false;
    };
    if ($('#fromDate3').val() == "") {
        alert("Please select valid from date.");
        return false;
    };

    var btnAction = $("#btnActionType").val();
    var thedata = $(this).serialize() + '&d1=' + date1 + '&d2=' + date2;
    $.ajax({
        url: this.action,
        type: "POST",
        data: thedata,
        success: function (result) {
            if (result.success) {
                if (btnAction == "SaveAndNext") {

                    alert("Data Saved.");
                }

                if (btnAction == "SaveAndClose") {
                    $("#additemModal").modal('hide');

                    if (result.redirectUrl != null) {
                        window.location = result.redirectUrl;
                    }

                }
            }
        }
    });
});


$('#clockInForm').submit(function (e) {
    e.preventDefault();
    //   var sorid = $("#sorNumber").val().trim();
    var stuId = $('#txtPersonID').val().trim();
    var btnAction = $("#btnActionType").val();
    $.ajax({
        url: this.action,
        type: "POST",
        data: $(this).serialize(),
        success: function (result) {
            if (result.success) {


                $("#additemModal").modal('hide');

            }
        }
    });
});
