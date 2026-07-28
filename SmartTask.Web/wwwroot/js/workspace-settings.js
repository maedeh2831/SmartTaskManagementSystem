document.addEventListener("DOMContentLoaded", function () {

    // ===== Logo Preview =====
    const logoInput = document.getElementById("LogoFile");
    const logoPreviewImg = document.getElementById("logoPreviewImg");
    const logoPreviewIcon = document.getElementById("logoPreviewIcon");

    if (logoInput) {
        logoInput.addEventListener("change", function () {
            const file = this.files[0];
            if (!file) return;

            const reader = new FileReader();
            reader.onload = function (e) {
                logoPreviewImg.src = e.target.result;
                logoPreviewImg.style.display = "block";
                if (logoPreviewIcon) logoPreviewIcon.style.display = "none";
            };
            reader.readAsDataURL(file);
        });
    }

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

});