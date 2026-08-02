document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".delete-label-form").forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            Swal.fire({
                title: "حذف Label",
                text: "آیا از حذف این Label مطمئن هستید؟ از تمام Taskهای مرتبط هم حذف می‌شود.",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "بله، حذف کن",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#EF4444",
                cancelButtonColor: "#64748B"
            }).then((result) => {
                if (result.isConfirmed) form.submit();
            });
        });
    });
});