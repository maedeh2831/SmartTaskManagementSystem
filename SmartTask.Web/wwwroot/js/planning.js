document.addEventListener("DOMContentLoaded", function () {

    const backlogColumn = document.getElementById("backlogColumn");
    const sprintColumn = document.getElementById("sprintColumn");
    const capacityFill = document.getElementById("capacityFill");
    const capacityText = document.getElementById("capacityText");
    const sprintCount = document.getElementById("sprintCount");
    const backlogCount = document.getElementById("backlogCount");
    const capacityBar = document.getElementById("capacityBar");

    if (!backlogColumn || !sprintColumn || typeof Sortable === "undefined") return;

    const sprintId = sprintColumn.dataset.sprintId;
    const token = document.querySelector('#antiForgeryForm input[name="__RequestVerificationToken"]');

    function getToken() {
        return token ? token.value : "";
    }

    function updateCapacityUI() {
        const capacity = parseInt(capacityBar.dataset.capacity, 10) || 0;

        const sprintCards = sprintColumn.querySelectorAll(".planning-card");
        const backlogCards = backlogColumn.querySelectorAll(".planning-card");

        let totalPoints = 0;
        sprintCards.forEach(c => totalPoints += parseInt(c.dataset.points, 10) || 0);

        sprintCount.innerText = sprintCards.length;
        backlogCount.innerText = backlogCards.length;
        capacityText.innerText = `${totalPoints} از ${capacity} Story Point`;

        const percent = capacity > 0 ? Math.round((totalPoints / capacity) * 100) : 0;
        capacityFill.style.width = `${Math.min(percent, 100)}%`;
        capacityFill.classList.toggle("over", percent > 100);
    }

    async function postAction(url, storyId) {
        const formData = new FormData();
        formData.append("storyId", storyId);
        formData.append("sprintId", sprintId);
        formData.append("__RequestVerificationToken", getToken());

        try {
            const response = await fetch(url, { method: "POST", body: formData });

            if (!response.ok) {
                const text = await response.text();
                console.error(`Server error [${response.status}] on ${url}:`, text);
                Swal.fire({
                    icon: "error",
                    title: `خطا (${response.status})`,
                    text: response.status === 403
                        ? "شما اجازه انجام این عملیات را ندارید."
                        : "ذخیره تغییرات با مشکل مواجه شد.",
                    confirmButtonColor: "#5B5FEF"
                });
                return false;
            }

            return true;
        } catch (err) {
            console.error("Network error:", err);
            Swal.fire({
                icon: "error",
                title: "خطای شبکه",
                text: "اتصال به سرور برقرار نشد.",
                confirmButtonColor: "#5B5FEF"
            });
            return false;
        }
    }

    // ===== وقتی آیتمی وارد ستون Sprint می‌شود (از Backlog اومده) =====
    function onAddToSprint(evt) {
        const storyId = evt.item.dataset.id;
        postAction("/Sprint/AssignToSprint", storyId);
        updateCapacityUI();
    }

    // ===== وقتی آیتمی وارد ستون Backlog می‌شود (از Sprint اومده) =====
    function onAddToBacklog(evt) {
        const storyId = evt.item.dataset.id;
        postAction("/Sprint/RemoveFromSprintPlanning", storyId);
        updateCapacityUI();
    }

    Sortable.create(backlogColumn, {
        group: "planning",
        animation: 150,
        handle: ".planning-drag-handle",
        ghostClass: "planning-ghost",
        chosenClass: "planning-chosen",
        onAdd: onAddToBacklog,
        onSort: updateCapacityUI
    });

    Sortable.create(sprintColumn, {
        group: "planning",
        animation: 150,
        handle: ".planning-drag-handle",
        ghostClass: "planning-ghost",
        chosenClass: "planning-chosen",
        onAdd: onAddToSprint,
        onSort: updateCapacityUI
    });

    updateCapacityUI();

});