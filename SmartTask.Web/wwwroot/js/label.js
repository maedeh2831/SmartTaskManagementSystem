// ==========================================================
//                 SmartTask Label UI
// ==========================================================

document.addEventListener("DOMContentLoaded", function () {

    // ===== Persian digits helper =====
    function toFaDigits(str) {
        var fa = "\u06F0\u06F1\u06F2\u06F3\u06F4\u06F5\u06F6\u06F7\u06F8\u06F9";
        return String(str).replace(/[0-9]/g, function (d) { return fa[+d]; });
    }


    // ===== Color picker preview sync =====
    var colorInput = document.getElementById("labelColorInput");
    var colorPreview = document.getElementById("labelColorPreview");

    if (colorInput && colorPreview) {
        colorInput.addEventListener("input", function () {
            colorPreview.style.background = this.value;
        });
    }


    // ===== Create form: loading state + validation =====
    var createForm = document.getElementById("labelCreateForm");
    var createBtn = document.getElementById("labelCreateBtn");
    var nameInput = document.getElementById("labelNameInput");
    var formError = document.getElementById("labelFormError");
    var formErrorText = document.getElementById("labelFormErrorText");

    if (createForm) {
        createForm.addEventListener("submit", function (e) {
            var name = nameInput ? nameInput.value.trim() : "";

            if (!name) {
                e.preventDefault();
                showFormError("نام Label را وارد کنید.");
                if (nameInput) nameInput.focus();
                return;
            }

            if (name.length > 100) {
                e.preventDefault();
                showFormError("نام Label نباید بیشتر از ۱۰۰ کاراکتر باشد.");
                return;
            }

            // Check for duplicate names
            var existingNames = [];
            document.querySelectorAll("#labelGrid .label-card").forEach(function (card) {
                existingNames.push((card.dataset.name || "").toLowerCase());
            });
            if (existingNames.indexOf(name.toLowerCase()) !== -1) {
                e.preventDefault();
                showFormError("Label\u200cای با این نام قبلاً وجود دارد.");
                if (nameInput) nameInput.focus();
                return;
            }

            // Show loading
            if (createBtn) {
                createBtn.classList.add("loading");
                createBtn.disabled = true;
            }
        });

        if (nameInput) {
            nameInput.addEventListener("input", function () {
                hideFormError();
            });
        }
    }

    function showFormError(msg) {
        if (formError && formErrorText) {
            formErrorText.textContent = msg;
            formError.classList.add("visible");
            if (createForm) createForm.classList.add("has-error");
        }
    }

    function hideFormError() {
        if (formError) {
            formError.classList.remove("visible");
            if (createForm) createForm.classList.remove("has-error");
        }
    }


    // ===== Search filter =====
    var searchInput = document.getElementById("labelSearchInput");
    var labelCards = document.querySelectorAll("#labelGrid .label-card");
    var noResults = document.getElementById("labelNoResults");

    function applyLabelSearch() {
        var q = searchInput ? searchInput.value.trim().toLowerCase() : "";
        var visible = 0;

        labelCards.forEach(function (card) {
            var name = (card.dataset.name || "").toLowerCase();
            var match = !q || name.indexOf(q) !== -1;
            card.style.display = match ? "" : "none";
            if (match) visible++;
        });

        if (noResults) {
            if (visible === 0 && labelCards.length > 0) {
                noResults.classList.add("visible");
            } else {
                noResults.classList.remove("visible");
            }
        }
    }

    if (searchInput) {
        searchInput.addEventListener("input", applyLabelSearch);
    }


    // ===== Delete with confirmation + animation =====
    document.querySelectorAll(".delete-label-form").forEach(function (form) {
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            var card = form.closest(".label-card");

            Swal.fire({
                title: "حذف Label",
                text: "آیا از حذف این Label مطمئن هستید؟ از تمام Taskهای مرتبط هم حذف می‌شود.",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "بله، حذف کن",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#EF4444",
                cancelButtonColor: "#64748B"
            }).then(function (result) {
                if (result.isConfirmed) {
                    if (card) {
                        card.classList.add("removing");
                        card.addEventListener("animationend", function () {
                            form.submit();
                        });
                    } else {
                        form.submit();
                    }
                }
            });
        });
    });


    // ===== Inline Edit =====
    document.querySelectorAll(".label-edit-btn").forEach(function (btn) {
        btn.addEventListener("click", function () {
            var card = btn.closest(".label-card");
            if (!card) return;

            var nameEl = card.querySelector(".label-name");
            var actionsEl = card.querySelector(".label-card-actions");
            var currentName = btn.dataset.name || (nameEl ? nameEl.textContent.trim() : "");
            var currentColor = btn.dataset.color || "#5B5FEF";
            var labelId = btn.dataset.id;

            if (nameEl) nameEl.style.display = "none";
            if (actionsEl) actionsEl.style.display = "none";

            var editForm = document.createElement("div");
            editForm.className = "label-edit-form";
            editForm.innerHTML =
                '<input type="text" class="label-edit-input" value="' + escapeAttr(currentName) + '" maxlength="100" />' +
                '<div class="label-edit-actions">' +
                    '<button type="button" class="label-edit-save" title="ذخیره"><i class="fa-solid fa-check"></i></button>' +
                    '<button type="button" class="label-edit-cancel" title="انصراف"><i class="fa-solid fa-xmark"></i></button>' +
                '</div>';

            card.insertBefore(editForm, actionsEl ? actionsEl.nextSibling : null);

            var input = editForm.querySelector(".label-edit-input");
            if (input) {
                input.focus();
                input.select();
            }

            editForm.querySelector(".label-edit-save").addEventListener("click", function () {
                var newName = input ? input.value.trim() : "";
                if (!newName || newName === currentName) {
                    cancelEdit();
                    return;
                }

                var token = document.querySelector('input[name="__RequestVerificationToken"]');
                var tokenValue = token ? token.value : "";

                var formData = new FormData();
                formData.append("id", labelId);
                formData.append("name", newName);
                formData.append("color", currentColor);
                formData.append("__RequestVerificationToken", tokenValue);

                fetch("/Label/Edit", { method: "POST", body: formData })
                    .then(function (r) {
                        if (r.ok || r.redirected) {
                            if (nameEl) {
                                nameEl.textContent = newName;
                                nameEl.style.display = "";
                            }
                            if (actionsEl) actionsEl.style.display = "";
                            editForm.remove();
                            card.dataset.name = newName.toLowerCase();
                            btn.dataset.name = newName;
                        } else {
                            cancelEdit();
                        }
                    })
                    .catch(function () {
                        cancelEdit();
                    });
            });

            editForm.querySelector(".label-edit-cancel").addEventListener("click", cancelEdit);

            input.addEventListener("keydown", function (e) {
                if (e.key === "Enter") {
                    e.preventDefault();
                    editForm.querySelector(".label-edit-save").click();
                } else if (e.key === "Escape") {
                    cancelEdit();
                }
            });

            function cancelEdit() {
                if (nameEl) nameEl.style.display = "";
                if (actionsEl) actionsEl.style.display = "";
                editForm.remove();
            }
        });
    });

    function escapeAttr(str) {
        return String(str)
            .replace(/&/g, "&amp;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;");
    }

});