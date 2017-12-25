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
        $.ajax({
            url: this.action,
            type: this.method,
            data: $(this).serialize(),
            success: function (result) {
                if (result.success) {
                    $('#myModal').modal('hide');
                    $('#progress').hide();
                    location.reload();
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


$('#btnDiscount').click(function () {

    if ($("#txtInputValue").val().length > 0) {
        var InvID = $('#txtInvID').val();
        var newDiscount = $('#txtDiscount').val();
        var newAmount = $('#txtAmount').val();
        var newGst = $('#txtGst').val();
        var newNett = $('#txtNett').val();

        // alert(newAmount);

        $.ajax({
            type: 'GET',
            url: '/Sales/_AddOverallDiscount',
            dataType: "json",
            contentType: 'application/json, charset=utf-8',
            data: {
                valInvID: InvID, valDiscount: newDiscount, valAmount: newAmount, valGst: newGst, valNett: newNett
            },
            success: function (result) {

                if (result.success) {
                    $('#discountModal').modal('hide');
                    $('#progress').hide();
                    //  alert("hello 1");
                    location.reload();
                } else {
                    $('#progress').hide();
                    $('#discountModalContent').html(result);
                    //   alert("hello 2");
                }


            },
            error: function (xhr, status, error) {
                alert(error);
            }

        });
    } else {
        alert("Not valid input.");
    };


});


$('#addDetForm').submit(function (e) {
    e.preventDefault();

    $.ajax({
        url: this.action,
        type: "POST",
        data: $(this).serialize(),
        success: function (result) {
            $("#Div").html(result);
            if (result.redirectUrl != null) {
                window.location = result.redirectUrl;
            }
        }

    });
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