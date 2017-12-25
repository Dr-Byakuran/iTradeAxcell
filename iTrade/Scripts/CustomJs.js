function removeNestedForm(element, container, deleteElement) {
    $container = $(element).parents(container);
    $container.find(deleteElement).val('True');
    $container.hide();
}

function addNestedForm(container, counter, ticks, content) {
    var nextIndex = $(counter).length;
    var pattern = new RegExp(ticks, "gi");
    content = content.replace(pattern, nextIndex);
    $(container).append(content);


    $('#infoBundles input[id*=txtSearch]').autocomplete({
        source: function (request, response) {
            $.ajax({
                url: '@Url.Action("AutoComplete_Product")',
                dataType: "json",
                contentType: 'application/json, charset=utf-8',
                data: {
                    search: $('#infoBundles input[id*=txtSearch]').val()
                },
                success: function (data) {

                    response($.map(data, function (item) {
                        return {
                            label: item.SKU + " | " + item.ProductName + " | " + item.Brand + " | $ " + item.CostPrice,

                        };
                    }));

                },
                error: function (xhr, status, error) {
                    alert(error);
                }

            });
        },
        selectFirst: true,
        delay: 0,
        autoFocus: true,
        minLength: 1


    });

    $('#infoBundles input[id*=txtSearch]').on("autocompleteselect", function (event, ui) {
        var str1 = ui.item.value;
        var str2 = str1.split("|");
        var val = str2[0].trim();

        //  alert(val);

        $.ajax({
            type: 'GET',
            url: '@Url.Action("AutoCompleteSelected_Product")',
            dataType: "json",
            contentType: 'application/json, charset=utf-8',
            data: {
                search: val
            },
            success: function (data) {
                $('#itemID').val(data.result.VariantID);
                $('#txtSKU').val(data.result.SKU);
                $('#itemType').val(data.result.ProductType);
                $('#txtProductName').val(data.result.ProductName + ' ' + data.result.VariantName);
                $('#itemUnit').val(data.result.Unit);
                var unitprice = data.result.SellPrice.toFixed(2);
                $('#itemUnitPrice').val(unitprice);
                $('#itemStockQty').val(0);

                $('#itemCostPrice').val(data.result.CostPrice);
                var disc = (0.00).toFixed(2);
                $('#itemDiscount').val(disc);
                $('#itemDiscountedPrice').val(unitprice);

                var qty = $('#txtQty').val();

                var amount = Math.round(qty * unitprice * 100) / 100;
                amount = amount.toFixed(2);

                $('#itemAmount').val(amount);

                $('#txtQty').focus();
                $('#txtQty').select();
                $('.infoBundle input.txtKey').val("");
                $('#searchbox').hide();

            },
            error: function (xhr, status, error) {
                alert(error);
            }

        });

    });





}