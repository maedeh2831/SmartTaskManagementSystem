document.addEventListener("DOMContentLoaded", function () {

    const list = document.querySelector(".offroad-list");
    if (!list) return;

    const projectId = list.dataset.projectId;
    const token = document.querySelector('.offroad-quick-add-form input[name="__RequestVerificationToken"]').value;

    document.querySelectorAll(".offroad-status-select").forEach(select => {
        select.addEventListener("change", async function () {
            const id = select.dataset.id;
            const row = select.closest(".offroad-row");

            await fetch("/Offroad/ChangeStatus", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: `id=${id}&projectId=${projectId}&status=${select.value}&__RequestVerificationToken=${encodeURIComponent(token)}`
            });

            row.classList.toggle("done", select.value == "3");
        });
    });

    document.querySelectorAll(".offroad-priority-select").forEach(select => {
        select.addEventListener("change", async function () {
            const id = select.dataset.id;

            await fetch("/Offroad/ChangePriority", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: `id=${id}&projectId=${projectId}&priority=${select.value}&__RequestVerificationToken=${encodeURIComponent(token)}`
            });
        });
    });

    document.querySelectorAll(".delete-offroad-form").forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            Swal.fire({
                title: "حذف کار آفرود",
                text: "آیا از حذف این آیتم مطمئن هستید؟",
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