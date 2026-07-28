document.addEventListener("DOMContentLoaded", function () {

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

    // ===== Invite Autocomplete (Multi-Select) =====
    const searchInput = document.getElementById("userSearchInput");
    if (!searchInput) return;

    const resultsBox = document.getElementById("userSearchResults");
    const chipsContainer = document.getElementById("selectedChipsContainer");
    const hiddenInputsContainer = document.getElementById("hiddenInputsContainer");
    const submitBtn = document.getElementById("inviteSubmitBtn");
    const workspaceId = document.querySelector('input[name="WorkspaceId"]').value;

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    let selectedUsers = [];   // { id, fullName, email, avatar }
    let selectedEmails = [];  // string[]

    let debounceTimer;

    searchInput.addEventListener("input", function () {
        clearTimeout(debounceTimer);
        const term = searchInput.value.trim();

        if (term.length < 2) {
            resultsBox.innerHTML = "";
            resultsBox.classList.remove("active");
            return;
        }

        debounceTimer = setTimeout(() => {
            fetch(`/WorkspaceMember/SearchUsers?workspaceId=${workspaceId}&term=${encodeURIComponent(term)}`)
                .then(res => res.json())
                .then(users => renderResults(users, term))
                .catch(() => {
                    resultsBox.innerHTML = "";
                });
        }, 300);
    });

    searchInput.addEventListener("keydown", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            const term = searchInput.value.trim();
            if (emailRegex.test(term)) {
                addEmailChip(term);
                searchInput.value = "";
                resultsBox.innerHTML = "";
                resultsBox.classList.remove("active");
            }
        }
    });

    function renderResults(users, term) {
        resultsBox.innerHTML = "";

        const alreadySelectedIds = selectedUsers.map(u => u.id);
        const filteredUsers = users.filter(u => !alreadySelectedIds.includes(u.id));

        filteredUsers.forEach(user => {
            const item = document.createElement("div");
            item.className = "autocomplete-item";
            item.innerHTML = `
                <div class="member-avatar">
                    ${user.avatar
                    ? `<img src="${user.avatar}" alt="${user.fullName}" />`
                    : '<i class="fa-solid fa-user"></i>'}
                </div>
                <div>
                    <h4>${user.fullName}</h4>
                    <span>${user.email}</span>
                </div>
            `;
            item.addEventListener("click", () => {
                addUserChip(user);
                searchInput.value = "";
                resultsBox.innerHTML = "";
                resultsBox.classList.remove("active");
            });
            resultsBox.appendChild(item);
        });

        if (emailRegex.test(term) && !selectedEmails.includes(term.toLowerCase())) {
            const emailItem = document.createElement("div");
            emailItem.className = "autocomplete-item new-email-option";
            emailItem.innerHTML = `
                <div class="member-avatar pending-avatar">
                    <i class="fa-solid fa-envelope"></i>
                </div>
                <div>
                    <h4>دعوت «${term}» به عنوان کاربر جدید</h4>
                    <span>یک ایمیل دعوت برای ثبت‌نام ارسال می‌شود</span>
                </div>
            `;
            emailItem.addEventListener("click", () => {
                addEmailChip(term);
                searchInput.value = "";
                resultsBox.innerHTML = "";
                resultsBox.classList.remove("active");
            });
            resultsBox.appendChild(emailItem);
        }

        if (!filteredUsers.length && !emailRegex.test(term)) {
            resultsBox.innerHTML = '<div class="autocomplete-empty">کاربری یافت نشد.</div>';
        }

        resultsBox.classList.add("active");
    }

    function addUserChip(user) {
        if (selectedUsers.some(u => u.id === user.id)) return;
        selectedUsers.push(user);
        renderChipsAndInputs();
    }

    function addEmailChip(email) {
        const normalized = email.toLowerCase();
        if (selectedEmails.includes(normalized)) return;
        selectedEmails.push(normalized);
        renderChipsAndInputs();
    }

    function removeUserChip(id) {
        selectedUsers = selectedUsers.filter(u => u.id !== id);
        renderChipsAndInputs();
    }

    function removeEmailChip(email) {
        selectedEmails = selectedEmails.filter(e => e !== email);
        renderChipsAndInputs();
    }

    function renderChipsAndInputs() {
        chipsContainer.innerHTML = "";
        hiddenInputsContainer.innerHTML = "";

        selectedUsers.forEach(user => {
            const chip = document.createElement("div");
            chip.className = "selected-chip";
            chip.innerHTML = `
                <div class="member-avatar">
                    ${user.avatar
                    ? `<img src="${user.avatar}" alt="${user.fullName}" />`
                    : '<i class="fa-solid fa-user"></i>'}
                </div>
                <span>${user.fullName}</span>
                <button type="button" class="chip-remove-btn">
                    <i class="fa-solid fa-xmark"></i>
                </button>
            `;
            chip.querySelector(".chip-remove-btn")
                .addEventListener("click", () => removeUserChip(user.id));
            chipsContainer.appendChild(chip);

            const input = document.createElement("input");
            input.type = "hidden";
            input.name = "UserIds";
            input.value = user.id;
            hiddenInputsContainer.appendChild(input);
        });

        selectedEmails.forEach(email => {
            const chip = document.createElement("div");
            chip.className = "selected-chip new-email-chip";
            chip.innerHTML = `
                <div class="member-avatar pending-avatar">
                    <i class="fa-solid fa-envelope"></i>
                </div>
                <span>${email}</span>
                <button type="button" class="chip-remove-btn">
                    <i class="fa-solid fa-xmark"></i>
                </button>
            `;
            chip.querySelector(".chip-remove-btn")
                .addEventListener("click", () => removeEmailChip(email));
            chipsContainer.appendChild(chip);

            const input = document.createElement("input");
            input.type = "hidden";
            input.name = "Emails";
            input.value = email;
            hiddenInputsContainer.appendChild(input);
        });

        submitBtn.disabled = selectedUsers.length === 0 && selectedEmails.length === 0;
    }

    document.addEventListener("click", function (e) {
        if (!resultsBox.contains(e.target) && e.target !== searchInput) {
            resultsBox.classList.remove("active");
        }
    });

});