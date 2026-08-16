document.addEventListener("DOMContentLoaded", function () {

    const sidebarToggle = document.getElementById("sidebarToggle");
    const sidebar = document.getElementById("sidebar");

    if (sidebarToggle && sidebar) {

        sidebarToggle.addEventListener("click", function () {

            sidebar.classList.toggle("collapsed");

        });

    }

});

function showSuccess(message) {

    Swal.fire({
        icon: 'success',
        title: 'موفق',
        text: message,
        confirmButtonText: 'باشه'
    });

}

function showError(message) {

    Swal.fire({
        icon: 'error',
        title: 'خطا',
        text: message,
        confirmButtonText: 'باشه'
    });

}

function showWarning(message) {

    Swal.fire({
        icon: 'warning',
        title: 'هشدار',
        text: message,
        confirmButtonText: 'باشه'
    });

    
}

document.addEventListener("DOMContentLoaded", function () {

    const logoutButton = document.getElementById("logoutButton");

    if (logoutButton) {

        logoutButton.addEventListener("click", function () {

            Swal.fire({

                title: "خروج از حساب",
                text: "آیا مطمئن هستید؟",
                icon: "question",

                showCancelButton: true,

                confirmButtonText: "بله، خارج شو",

                cancelButtonText: "انصراف",

                confirmButtonColor: "#4f46e5",

                cancelButtonColor: "#64748b"

            }).then((result) => {

                if (result.isConfirmed) {

                    document
                        .getElementById("logoutForm")
                        .submit();

                }

            });

        });

    }

    document.querySelectorAll(".delete-form")
        .forEach(form => {

            console.log("DELETE FORM FOUND");

            form.addEventListener("submit", function (e) {

                console.log("DELETE CLICKED");

                e.preventDefault();

                Swal.fire({

                    title: "حذف Workspace",

                    text: "آیا از حذف این فضای کاری مطمئن هستید؟",

                    icon: "warning",

                    showCancelButton: true,

                    confirmButtonText: "بله، حذف کن",

                    cancelButtonText: "انصراف",

                    confirmButtonColor: "#EF4444",

                    cancelButtonColor: "#64748B"

                }).then((result) => {

                    if (result.isConfirmed) {

                        form.submit();

                    }

                });

            });

        });

});

window.SmartTask = window.SmartTask || {};
SmartTask.toggleSidebarGroup = function (linkEl) {
    var li = linkEl.closest("li.has-children");
    if (li) {
        li.classList.toggle("open");
    }
    return false;
};