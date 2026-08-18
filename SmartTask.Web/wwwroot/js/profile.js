document.addEventListener("DOMContentLoaded", function () {

    const avatarInput =
        document.getElementById("avatarInput");

    const avatarPreview =
        document.getElementById("avatarPreview");

    const avatarPlaceholder =
        document.getElementById("avatarPlaceholder");

    const bio =
        document.getElementById("Bio");

    const bioCounter =
        document.getElementById("bioCounter");

    const removeAvatarButton =
        document.getElementById("removeAvatarButton");


    // Avatar Preview
    if (avatarInput) {

        avatarInput.addEventListener("change", function () {

            const file = this.files[0];

            if (!file)
                return;

            if (file.size > 2 * 1024 * 1024) {

                showError(
                    "حجم تصویر نباید بیشتر از ۲ مگابایت باشد."
                );

                this.value = "";

                return;
            }


            const allowedTypes = [
                "image/jpeg",
                "image/png",
                "image/webp"
            ];

            if (!allowedTypes.includes(file.type)) {

                showError(
                    "فرمت تصویر انتخاب‌شده مجاز نیست."
                );

                this.value = "";

                return;
            }


            const reader = new FileReader();

            reader.onload = function (e) {

                avatarPreview.src = e.target.result;

                avatarPreview.classList.remove("d-none");

            };

            reader.readAsDataURL(file);

        });

    }


    // Bio Counter
    if (bio && bioCounter) {

        function updateBioCounter() {

            bioCounter.textContent =
                bio.value.length;

        }

        bio.addEventListener(
            "input",
            updateBioCounter
        );

        updateBioCounter();

    }


    // Remove Avatar
    if (removeAvatarButton) {

        removeAvatarButton.addEventListener(
            "click",
            function () {

                Swal.fire({

                    title: "حذف تصویر پروفایل؟",

                    text:
                        "تصویر فعلی از حساب شما حذف خواهد شد.",

                    icon: "warning",

                    showCancelButton: true,

                    confirmButtonText:
                        "بله، حذف کن",

                    cancelButtonText:
                        "انصراف",

                    confirmButtonColor:
                        "#EF4444",

                    cancelButtonColor:
                        "#64748B"

                }).then(function (result) {

                    if (result.isConfirmed) {

                        document
                            .getElementById("removeAvatarForm")
                            .submit();

                    }

                });

            }
        );

    }

});