document.addEventListener("DOMContentLoaded", function () {

    // ===== Color Swatches =====
    const colorSwatches = document.querySelectorAll(".color-swatch");
    const colorPicker = document.getElementById("ColorPicker");

    colorSwatches.forEach(function (swatch) {
        swatch.addEventListener("click", function () {
            const color = this.getAttribute("data-color");
            colorPicker.value = color;

            colorSwatches.forEach(s => s.classList.remove("active"));
            this.classList.add("active");
        });
    });

    if (colorPicker) {
        colorPicker.addEventListener("input", function () {
            colorSwatches.forEach(s => s.classList.remove("active"));
        });
    }

    // ===== Icon Swatches =====
    const iconSwatches = document.querySelectorAll(".icon-swatch");
    const iconInput = document.getElementById("IconInput");

    iconSwatches.forEach(function (swatch) {
        swatch.addEventListener("click", function () {
            const icon = this.getAttribute("data-icon");
            iconInput.value = icon;

            iconSwatches.forEach(s => s.classList.remove("active"));
            this.classList.add("active");
        });
    });

    // ===== Archive Confirmation =====
    const btnArchive = document.getElementById("btnArchive");
    if (btnArchive) {
        btnArchive.addEventListener("click", function () {
            Swal.fire({
                title: "بایگانی پروژه؟",
                text: "پروژه غیرفعال می‌شود اما می‌توانید بعداً آن را بازگردانید.",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "بله، بایگانی شود",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#F59E0B"
            }).then(function (result) {
                if (result.isConfirmed) {
                    document.getElementById("archiveForm").submit();
                }
            });
        });
    }

    // ===== Restore Confirmation =====
    const btnRestore = document.getElementById("btnRestore");
    if (btnRestore) {
        btnRestore.addEventListener("click", function () {
            Swal.fire({
                title: "بازگردانی پروژه؟",
                text: "پروژه دوباره فعال خواهد شد.",
                icon: "question",
                showCancelButton: true,
                confirmButtonText: "بله، بازگردانی شود",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#16A34A"
            }).then(function (result) {
                if (result.isConfirmed) {
                    document.getElementById("restoreForm").submit();
                }
            });
        });
    }

    // ===== Delete Confirmation =====
    const btnDelete = document.getElementById("btnDelete");
    if (btnDelete) {
        btnDelete.addEventListener("click", function () {
            Swal.fire({
                title: "حذف پروژه؟",
                text: "این عملیات غیرقابل بازگشت است!",
                icon: "error",
                showCancelButton: true,
                confirmButtonText: "بله، حذف شود",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#DC2626"
            }).then(function (result) {
                if (result.isConfirmed) {
                    document.getElementById("deleteForm").submit();
                }
            });
        });
    }

});