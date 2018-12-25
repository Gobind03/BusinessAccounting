// GET PRODUCTS
var products_arr = [];
var products_options = "";

var tax_arr = [];
var tax_options = "";

$.ajax({
    url: "/api/panel/ProductsList",
    type: "GET",
    success: function (result) {
        products_arr = result;
        for (var i = 0; i < result.length; i++) {
            products_options += "<option value='" + result[i].id_product_master + "'>" + result[i].title + "</option>";
        }
        // GET TAX
        $.ajax({
            url: "/api/panel/TaxList",
            type: "GET",
            success: function (result) {
                tax_arr = result;
                for (var i = 0; i < result.length; i++) {
                    tax_options += "<option value='" + result[i].id_tax_master + "'>" + result[i].label + "</option>";
                }
                addRow();
            }
        });
    }
});


// GET COUNTRY
var country_arr = [];
$.ajax({
    url: "/api/panel/CountryList",
    type: "GET",
    success: function (result) {
        country_arr = result;
        for (var i = 0; i < result.length; i++) {
            $("#countryList").append("<option value='" + result[i].id_country + "'>" + result[i].currency + "</option>")
        }
    }
});

var list_arr = [];
function addRow() {
    var index = (list_arr).length;
    list_arr.push({
        products: `<select id="product` + index + `" name="invoiceDetailList[` + index + `].product" required class="form-control form-control-sm" onChange="fetchProduct(` + index + `)">
                    <option value="">Choose One</option> 
                    `+ products_options + `</select>`,
        desc: `<textarea class="form-control form-control-sm" name="invoiceDetailList[` + index + `].description" placeholder="Description For Item"></textarea>`,
        quant: `<input id="quantity` + index + `" type="number" name="invoiceDetailList[` + index + `].quantity" onChange="calcPrice(` + index + `)" class="form-control form-control-sm" required placeholder="Quantity" />`,
        price: `<input id="price` + index + `" type="number" name="invoiceDetailList[` + index + `].price" class="form-control form-control-sm" readonly placeholder="Price" />`,
        tax: `<select name="invoiceDetailList[` + index + `].tax" class="form-control form-control-sm" id="tax` + index + `">
                    <option value="">-- Choose One --</option>
                    `+ tax_options + `    
              </select>`,
        amount: `<input class="form-control form-control-sm" name="invoiceDetailList[` + index + `].amount" readonly id='amount` + index + `' value="0">`,
        del: "<button class='btn btn-sm btn-danger' type='button'>Delete</button>"
    });

    populate_grid();
}


function populate_grid() {
    var x = "";
    for (var i = 0; i < (list_arr).length; i++) {
        var s = "";
        for (var key in list_arr[i]) {
            s += "<td>" + list_arr[i][key] + "</td>"
        }
        x += "<tr>" + s + "</tr>";
    }
    $("#list").html(x);
}


$("#addGrid").on('click', function () {
    addRow();
});


function fetchProduct(id) {
    $.ajax({
        url: "/api/panel/Product/" + $("#product" + id).val(),
        type: "GET",
        success: function (result) {
            $("#price" + id).val(result[0].price);
        }
    });
}

function calcPrice(id) {
    $("#amount" + id).val($("#quantity" + id).val() * $("#price" + id).val());
}
