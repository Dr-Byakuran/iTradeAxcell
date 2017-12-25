


    $(document).ready(function () {

        $('#commModal').on('shown.bs.modal', function () {
            var invAmount = $('#commModal #txtSalesAmount').val();
            var commRate = $('#commModal  #txtCommRate').val();
            var commTotal = (+invAmount * (+commRate / 100)).toFixed(2);

            $('#commModal #txtCommTotal').val(commTotal);
 
        });

        $('#btnDiscount').click(function () {
            if ($("#txtInputValue").val().length > 0) {
                var invNo = $('#txtInvno').val();
                var newDiscount = $('#txtDiscount').val();
                var newAmount = $('#txtAmount').val();
                var newGst = $('#txtGst').val();
                var newNett = $('#txtNett').val();
                //var newUrl = '@Url.Action("_OrderDetail", "Quotation", new { id = @Model.QuoID })';
                var newUrl = $('#txtUrl1').val();
                $.ajax({
                    type: 'GET',
                    url: '/'+Controller.Name+'/_AddOverallDiscount',//'/Quotation/_AddOverallDiscount',
                    dataType: "json",
                    contentType: 'application/json, charset=utf-8',
                    data: {
                        valInvID: invNo, valDiscount: newDiscount, valAmount: newAmount, valGst: newGst, valNett: newNett
                    },
                    success: function (result) {

                        if (result.success) {
                            $('#discountModal').modal('hide');
                            $('#progress').hide();
                            $("#divOrderDetail").load(newUrl);
                            //  alert("hello 1");
                            //location.reload();
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

            }
            else {
                alert("Not valid input.");
            };


        });




        $('#btnZeroRated').click(function () {
            var zerorate = $("input:radio[name='radioZeroRated']:checked").val();
            var SorNo = $('#gstSorNo').val();
            //var newUrl = '@Url.Action("_OrderDetail", "Quotation", new { id = @Model.QuoID })';
            var newUrl = $('#txtUrl1').val();
            if (SorNo > 0) {
                // alert(invNo + '**' + newDiscount + '**' + newAmount + '**' + newGst + '**' + newNett + '**' + newUrl);

                $.ajax({
                    type: 'GET',
                    url: '/' + Controller.Name + '/_ChangeGST',//'/Quotation/_ChangeGST',
                    dataType: "json",
                    contentType: 'application/json, charset=utf-8',
                    data: {
                        valSorID: SorNo, valZeroRated: zerorate
                    },
                    success: function (result) {

                        if (result.success) {
                            $('#gstModal').modal('hide');
                            $('#progress3').hide();
                            $("#divOrderDetail").load(newUrl);

                        } else {
                            $('#progress3').hide();
                            $('#gstModalContent').html(result);
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



    });




    $(document).ready(function () {
        
        $("#discountModal #txtInputValue").forceNumeric();
        $('#discountModal #txtInputValue').on("change", function () {
            //   alert('Changed!');
            reCalculateAmount();

        });

        $('input[type=radio][name=processType]').change(function() {

            reCalculateAmount();

        });

        function reCalculateAmount() {


            var prediscAmount = $('#divOrderDetail #invPreDiscAmount').val();//@Model.PreDiscAmount;
            var discount = $('#divOrderDetail #invDiscount').val();//@Model.Discount;
            var amount = $('#divOrderDetail #invAmount').val()//@Model.Amount;
            var gst = $('#divOrderDetail #invGst').val();//@Model.Gst;
            var nett = $('#divOrderDetail #invNett').val();// @Model.Nett;

            var gstrate = $('#txtGstRate').val();

            var txtinput = $('#txtInputValue').val();
            txtinput = Number(txtinput).toFixed(2);

            var discountType = $('input[type=radio][name=processType]:checked').val();

            switch(discountType) {
                case '1' :
                    // by amount
                    var newdiscount = 0 - txtinput;
                    var newamount = +prediscAmount - +txtinput;
                    var newgst = Math.round(newamount * gstrate * 100) / 100;
                    newgst = newgst.toFixed(2);
                    var newnett = +newamount + +newgst;
                    newnett = newnett.toFixed(2);

                    $('#txtPreDiscAmount').val(prediscAmount);
                    $('#txtDiscount').val(newdiscount);
                    $('#txtAmount').val(newamount);
                    $('#txtGst').val(newgst);
                    $('#txtNett').val(newnett);

                    break;
                case '2' :
                    // by percentage
                    var newdiscount = Math.round(prediscAmount * (txtinput / 100) * 100) / 100;
                    newdiscount = newdiscount.toFixed(2);
                    var newamount = prediscAmount - newdiscount;
                    var newgst = Math.round(newamount * gstrate * 100) / 100;
                    newgst = newgst.toFixed(2);
                    var newnett = +newamount + +newgst;
                    newnett = newnett.toFixed(2);

                    $('#txtPreDiscAmount').val(prediscAmount);
                    $('#txtDiscount').val(0 - newdiscount);
                    $('#txtAmount').val(newamount);
                    $('#txtGst').val(newgst);
                    $('#txtNett').val(newnett);

                    break;
                case '3' :
                    // offer a final amount that include GST
                    var gstrate2 = (gstrate * 100 + 100) / 100;
                    var newnett = txtinput;
                    var newamount = Math.round(newnett / gstrate2 * 100) / 100;
                    var newamount = newamount.toFixed(2);
                    var newgst = newnett - newamount;
                    newgst = newgst.toFixed(2);
                    var newdiscount = newamount - prediscAmount;
                    newdiscount = newdiscount.toFixed(2);

                    $('#txtPreDiscAmount').val(prediscAmount);
                    $('#txtDiscount').val(newdiscount);
                    $('#txtAmount').val(newamount);
                    $('#txtGst').val(newgst);
                    $('#txtNett').val(newnett);

                    break;
            }
        };
    });

    function DiscountModelUpdate()
    {
        var val1 = $('#divOrderDetail #invPreDiscAmount').val();
        var val2 = $('#divOrderDetail #invDiscount').val();
        var val3 = $('#divOrderDetail #invAmount').val();
        var val4 = $('#divOrderDetail #invGst').val();
        var val5 = $('#divOrderDetail #invNett').val();
        $('#crPreDiscAmount').val(val1);
        $('#crDiscount').val(val2);
        $('#crAmount').val(val3);
        $('#crGst').val(val4);
        $('#crNett').val(val5);
    }
