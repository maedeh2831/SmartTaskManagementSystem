document.addEventListener("DOMContentLoaded", function () {

    // ===== Invite Modal =====
    var openBtn = document.getElementById("openInviteModalBtn");
    var modalContainer = document.getElementById("inviteModalContainer");
    if (openBtn && modalContainer) {
        var bsModal = null;

        openBtn.addEventListener("click", function () {
            var workspaceId = this.dataset.workspaceId;
            fetch("/WorkspaceMember/Invite?workspaceId=" + workspaceId, {
                headers: { "X-Requested-With": "XMLHttpRequest" }
            })
            .then(function (res) { return res.text(); })
            .then(function (html) {
                modalContainer.innerHTML = html;
                var modalEl = document.getElementById("inviteModal");
                bsModal = new bootstrap.Modal(modalEl);
                bsModal.show();
                initInviteForm();
            });
        });

        function initInviteForm() {
            var form = document.getElementById("inviteForm");
            if (!form) return;

            var searchInput = document.getElementById("userSearchInput");
            var resultsBox = document.getElementById("userSearchResults");
            var chipsContainer = document.getElementById("selectedChipsContainer");
            var hiddenInputsContainer = document.getElementById("hiddenInputsContainer");
            var submitBtn = document.getElementById("inviteSubmitBtn");
            var workspaceIdInput = form.querySelector('input[name="WorkspaceId"]');
            var wsId = workspaceIdInput ? workspaceIdInput.value : "";
            var emailRegex = new RegExp("^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$");
            var selectedUsers = [];
            var selectedEmails = [];
            var debounceTimer;

            if (searchInput) {
                searchInput.addEventListener("input", function () {
                    clearTimeout(debounceTimer);
                    var term = searchInput.value.trim();
                    if (term.length < 2) { resultsBox.innerHTML = ""; resultsBox.classList.remove("active"); return; }
                    debounceTimer = setTimeout(function () {
                        fetch("/WorkspaceMember/SearchUsers?workspaceId=" + wsId + "&term=" + encodeURIComponent(term))
                            .then(function (res) { return res.json(); })
                            .then(function (users) { renderResults(users, term); })
                            .catch(function () { resultsBox.innerHTML = ""; });
                    }, 300);
                });
                searchInput.addEventListener("keydown", function (e) {
                    if (e.key === "Enter") {
                        e.preventDefault();
                        var term = searchInput.value.trim();
                        if (emailRegex.test(term)) { addEmailChip(term); searchInput.value = ""; resultsBox.innerHTML = ""; resultsBox.classList.remove("active"); }
                    }
                });
            }

            function renderResults(users, term) {
                resultsBox.innerHTML = "";
                var alreadySelectedIds = selectedUsers.map(function (u) { return u.id; });
                var filteredUsers = users.filter(function (u) { return alreadySelectedIds.indexOf(u.id) === -1; });
                filteredUsers.forEach(function (user) {
                    var item = document.createElement("div");
                    item.className = "autocomplete-item";
                    var avatarHtml = user.avatar ? '<img src="' + user.avatar + '" alt="' + user.fullName + '" />' : '<i class="fa-solid fa-user"></i>';
                    item.innerHTML = '<div class="member-avatar">' + avatarHtml + '</div><div><h4>' + user.fullName + '</h4><span>' + user.email + '</span></div>';
                    item.addEventListener("click", function () { addUserChip(user); searchInput.value = ""; resultsBox.innerHTML = ""; resultsBox.classList.remove("active"); });
                    resultsBox.appendChild(item);
                });
                if (emailRegex.test(term) && selectedEmails.indexOf(term.toLowerCase()) === -1) {
                    var emailItem = document.createElement("div");
                    emailItem.className = "autocomplete-item new-email-option";
                    emailItem.innerHTML = '<div class="member-avatar pending-avatar"><i class="fa-solid fa-envelope"></i></div><div><h4>دعوت \u00ab' + term + '\u00bb به عنوان کاربر جدید</h4><span>یک ایمیل دعوت برای ثبت\u200cنام ارسال می\u200cشود</span></div>';
                    emailItem.addEventListener("click", function () { addEmailChip(term); searchInput.value = ""; resultsBox.innerHTML = ""; resultsBox.classList.remove("active"); });
                    resultsBox.appendChild(emailItem);
                }
                if (!filteredUsers.length && !emailRegex.test(term)) { resultsBox.innerHTML = '<div class="autocomplete-empty">کاربری یافت نشد.</div>'; }
                resultsBox.classList.add("active");
            }

            function addUserChip(user) { if (selectedUsers.some(function (u) { return u.id === user.id; })) return; selectedUsers.push(user); renderChipsAndInputs(); }
            function addEmailChip(email) { var n = email.toLowerCase(); if (selectedEmails.indexOf(n) !== -1) return; selectedEmails.push(n); renderChipsAndInputs(); }
            function removeUserChip(id) { selectedUsers = selectedUsers.filter(function (u) { return u.id !== id; }); renderChipsAndInputs(); }
            function removeEmailChip(email) { selectedEmails = selectedEmails.filter(function (e) { return e !== email; }); renderChipsAndInputs(); }

            function renderChipsAndInputs() {
                chipsContainer.innerHTML = "";
                hiddenInputsContainer.innerHTML = "";
                selectedUsers.forEach(function (user) {
                    var chip = document.createElement("div");
                    chip.className = "selected-chip";
                    var avatarHtml = user.avatar ? '<img src="' + user.avatar + '" alt="' + user.fullName + '" />' : '<i class="fa-solid fa-user"></i>';
                    chip.innerHTML = '<div class="member-avatar">' + avatarHtml + '</div><span>' + user.fullName + '</span><button type="button" class="chip-remove-btn"><i class="fa-solid fa-xmark"></i></button>';
                    chip.querySelector(".chip-remove-btn").addEventListener("click", function () { removeUserChip(user.id); });
                    chipsContainer.appendChild(chip);
                    var input = document.createElement("input"); input.type = "hidden"; input.name = "UserIds"; input.value = user.id; hiddenInputsContainer.appendChild(input);
                });
                selectedEmails.forEach(function (email) {
                    var chip = document.createElement("div");
                    chip.className = "selected-chip new-email-chip";
                    chip.innerHTML = '<div class="member-avatar pending-avatar"><i class="fa-solid fa-envelope"></i></div><span>' + email + '</span><button type="button" class="chip-remove-btn"><i class="fa-solid fa-xmark"></i></button>';
                    chip.querySelector(".chip-remove-btn").addEventListener("click", function () { removeEmailChip(email); });
                    chipsContainer.appendChild(chip);
                    var input = document.createElement("input"); input.type = "hidden"; input.name = "Emails"; input.value = email; hiddenInputsContainer.appendChild(input);
                });
                submitBtn.disabled = selectedUsers.length === 0 && selectedEmails.length === 0;
            }

            document.addEventListener("click", function (e) {
                if (resultsBox && !resultsBox.contains(e.target) && e.target !== searchInput) { resultsBox.classList.remove("active"); }
            });

            form.addEventListener("submit", function (e) {
                e.preventDefault();
                var formData = new FormData(form);
                var summaryEl = document.getElementById("inviteValidationSummary");

                fetch(form.action, {
                    method: "POST",
                    body: formData,
                    headers: { "X-Requested-With": "XMLHttpRequest" }
                })
                .then(function (res) {
                    if (res.ok) return res.json();
                    return res.json().then(function (data) { throw data; });
                })
                .then(function (data) {
                    if (data.success) {
                        if (bsModal) bsModal.hide();
                        showSuccess(data.message);
                        setTimeout(function () { location.reload(); }, 500);
                    }
                })
                .catch(function (err) {
                    if (err.errors) {
                        var msgs = [];
                        for (var key in err.errors) {
                            msgs.push(err.errors[key].join(" "));
                        }
                        if (summaryEl) {
                            summaryEl.innerHTML = msgs.join("<br>");
                            summaryEl.style.display = "block";
                        }
                    } else if (err.message) {
                        showError(err.message);
                    }
                });
            });
        }
    }

    // ===== Remove Member =====
    document.querySelectorAll(".remove-member-form").forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            Swal.fire({
                title: "حذف عضو",
                text: "آیا از حذف این عضو مطمئن هستید؟",
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

    // ===== Cancel Invitation =====
    document.querySelectorAll(".cancel-invitation-form").forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            Swal.fire({
                title: "لغو دعوت‌نامه",
                text: "آیا از لغو این دعوت‌نامه مطمئن هستید؟",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "بله، لغو کن",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#EF4444",
                cancelButtonColor: "#64748B"
            }).then((result) => {
                if (result.isConfirmed) form.submit();
            });
        });
    });

    // ===== Change Role =====
    document.querySelectorAll(".change-role-form .role-select").forEach(select => {
        const originalValue = select.value;
        select.addEventListener("change", function () {
            const form = select.closest("form");
            Swal.fire({
                title: "تغییر نقش",
                text: "آیا از تغییر نقش این عضو مطمئن هستید؟",
                icon: "question",
                showCancelButton: true,
                confirmButtonText: "بله، تغییر بده",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#5B5FEF",
                cancelButtonColor: "#64748B"
            }).then((result) => {
                if (result.isConfirmed) form.submit();
                else select.value = originalValue;
            });
        });
    });


});