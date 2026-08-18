// ==========================================================
//            SmartTask - Sprint Planning board
// ==========================================================

window.SmartTask = window.SmartTask || {};

// Converts Latin digits to Persian digits (0-9 -> ۰-۹)
function toFaDigits(str) {
    var fa = "۰۱۲۳۴۵۶۷۸۹";
    return String(str).replace(/[0-9]/g, function (d) { return fa[+d]; });
}

// scope: element containing the board (or document for the standalone page)
SmartTask.initPlanning = function (scope) {
    scope = scope || document;

    const backlogColumn = scope.querySelector("#backlogColumn");
    const sprintColumn = scope.querySelector("#sprintColumn");
    const capacityFill = scope.querySelector("#capacityFill");
    const capacityText = scope.querySelector("#capacityText");
    const sprintCount = scope.querySelector("#sprintCount");
    const backlogCount = scope.querySelector("#backlogCount");
    const capacityBar = scope.querySelector("#capacityBar");

    if (!backlogColumn || !sprintColumn || typeof Sortable === "undefined") return;

    const sprintId = sprintColumn.dataset.sprintId;

    // Anti-forgery token: prefer the dedicated form, fall back to any
    // token already rendered on the page (e.g. the Details page hero).
    function getToken() {
        const el = document.querySelector('#antiForgeryForm input[name="__RequestVerificationToken"]')
            || document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : "";
    }

    function updateCapacityUI() {
        const capacity = parseInt(capacityBar.dataset.capacity, 10) || 0;

        const sprintCards = sprintColumn.querySelectorAll(".planning-card");
        const backlogCards = backlogColumn.querySelectorAll(".planning-card");

        let totalPoints = 0;
        sprintCards.forEach(c => totalPoints += parseInt(c.dataset.points, 10) || 0);

        sprintCount.innerText = toFaDigits(sprintCards.length);
        backlogCount.innerText = toFaDigits(backlogCards.length);
        capacityText.innerText = `${toFaDigits(totalPoints)} از ${toFaDigits(capacity)} Story Point`;

        const percent = capacity > 0 ? Math.round((totalPoints / capacity) * 100) : 0;
        capacityFill.style.width = `${Math.min(percent, 100)}%`;
        capacityFill.classList.toggle("over", percent > 100);
    }

    async function postAction(url, storyId, item, originalParent, originalNextSibling) {
        const formData = new FormData();
        formData.append("storyId", storyId);
        formData.append("sprintId", sprintId);
        formData.append("__RequestVerificationToken", getToken());

        try {
            const response = await fetch(url, { method: "POST", body: formData });

            if (!response.ok) {
                const text = await response.text();
                console.error(`Server error [${response.status}] on ${url}:`, text);

                // Rollback: restore card to original position
                if (originalParent && item) {
                    originalParent.insertBefore(item, originalNextSibling);
                    updateCapacityUI();
                }

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

            // Rollback: restore card to original position
            if (originalParent && item) {
                originalParent.insertBefore(item, originalNextSibling);
                updateCapacityUI();
            }

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
        const originalParent = evt.from;
        const originalNextSibling = evt.item.nextSibling;
        postAction("/Sprint/AssignToSprint", storyId, evt.item, originalParent, originalNextSibling);
        updateCapacityUI();
    }

    // ===== وقتی آیتمی وارد ستون Backlog می‌شود (از Sprint اومده) =====
    function onAddToBacklog(evt) {
        const storyId = evt.item.dataset.id;
        const originalParent = evt.from;
        const originalNextSibling = evt.item.nextSibling;
        postAction("/Sprint/RemoveFromSprintPlanning", storyId, evt.item, originalParent, originalNextSibling);
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
};

document.addEventListener("DOMContentLoaded", function () {
    if (document.getElementById("backlogColumn")) {
        SmartTask.initPlanning(document);
    }
});
