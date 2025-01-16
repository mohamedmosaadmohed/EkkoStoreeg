var dtble;
$(document).ready(function () {
    loaddata();
});
function loaddata() {
    dtble = $("#mytable").DataTable({
        "ajax": {
            "url": "/Admin/Region/GetData",
        },
        "columns": [
            { "data": "name" },
            { "data": "shippingCost" },
            {
                "data": "id",
                "render": function (data) {
                    return `<a href="/Admin/City/Index/${data}" class="btn btn-secondary"><i class="bi bi-info-circle"></i></a>`;
                },
                "orderable": false
            },
            {
                "data": "id",
                "render": function (data) {
                    return `<a href="#" class="btn btn-primary btn-update-region" data-id="${data}"><i class="bi bi-pencil-square"></i></a>`;
                },
                "orderable": false
            },
            {
                "data": "id",
                "render": function (data) {
                    return `<a onClick="DeleteItem('/Admin/Region/DeleteRegion/${data}')" class="btn btn-danger"><i class="bi bi-trash"></i></a>`;
                },
                "orderable": false
            }
        ]
    });
}
function DeleteItem(url) {
    // Determine if the current page is in RTL (Right-to-Left) mode
    var isRtl = document.documentElement.dir === "rtl";

    // Set text based on language direction
    var swalTitle = isRtl ? "هل أنت متأكد؟" : "Are you sure?";
    var swalText = isRtl ? "لن تتمكن من التراجع عن هذا!" : "You won't be able to revert this!";
    var confirmButtonText = isRtl ? "نعم، احذفه!" : "Yes, delete it!";
    var cancelButtonText = isRtl ? "إلغاء" : "Cancel";
    var deletedTitle = isRtl ? "تم الحذف!" : "Deleted!";
    var deletedText = isRtl ? "تم حذف المنتج." : "Your product has been deleted.";

    Swal.fire({
        title: swalTitle,
        text: swalText,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: confirmButtonText,
        cancelButtonText: cancelButtonText
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        dtble.ajax.reload();
                        toastr.success(isRtl ? "تمت العملية بنجاح." : data.message);
                    } else {
                        toastr.error(isRtl ? "حدث خطأ." : data.message);
                    }
                }
            });
            Swal.fire({
                title: deletedTitle,
                text: deletedText,
                icon: "success"
            });
        }
    });
}


