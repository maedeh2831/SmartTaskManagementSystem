document.addEventListener("DOMContentLoaded", function () {

    // ===== Remove Member =====
    document.querySelectorAll(".remove-member-form")
        .forEach(form => {
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
                    if (result.isConfirmed) {
                        form.submit();
                    }
                });
            });
        });

    // ===== Change Role =====
    document.querySelectorAll(".change-role-form .role-select")
        .forEach(select => {
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
                    if (result.isConfirmed) {
                        form.submit();
                    } else {
                        select.value = originalValue;
                    }
                });
            });
        });

    // ===== Invite Autocomplete =====
    const searchInput = document.getElementById("userSearchInput");
    if (!searchInput) return;

    const resultsBox = document.getElementById("userSearchResults");
    const selectedUserId = document.getElementById("selectedUserId");
    const selectedUserCard = document.getElementById("selectedUserCard");
    const selectedUserName = document.getElementById("selectedUserName");
    const selectedUserEmail = document.getElementById("selectedUserEmail");
    const selectedUserAvatar = document.getElementById("selectedUserAvatar");
    const clearBtn = document.getElementById("clearSelectedUser");
    const submitBtn = document.getElementById("inviteSubmitBtn");
    const workspaceId = document.querySelector('input[name="WorkspaceId"]').value;

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
                .then(users => renderResults(users))
                .catch(() => {
                    resultsBox.innerHTML = "";
                });
        }, 300);
    });

    function renderResults(users) {
        resultsBox.innerHTML = "";

        if (!users.length) {
            resultsBox.innerHTML = '<div class="autocomplete-empty">کاربری یافت نشد.</div>';
            resultsBox.classList.add("active");
            return;
        }

        users.forEach(user => {
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
            item.addEventListener("click", () => selectUser(user));
            resultsBox.appendChild(item);
        });

        resultsBox.classList.add("active");
    }

    function selectUser(user) {
        selectedUserId.value = user.id;
        selectedUserName.textContent = user.fullName;
        selectedUserEmail.textContent = user.email;
        selectedUserAvatar.innerHTML = user.avatar
            ? `<img src="${user.avatar}" alt="${user.fullName}" />`
            : '<i class="fa-solid fa-user"></i>';

        selectedUserCard.style.display = "flex";
        searchInput.value = "";
        resultsBox.innerHTML = "";
        resultsBox.classList.remove("active");
        submitBtn.disabled = false;
    }

    clearBtn.addEventListener("click", function () {
        selectedUserId.value = "";
        selectedUserCard.style.display = "none";
        submitBtn.disabled = true;
    });

    document.addEventListener("click", function (e) {
        if (!resultsBox.contains(e.target) && e.target !== searchInput) {
            resultsBox.classList.remove("active");
        }
    });

});