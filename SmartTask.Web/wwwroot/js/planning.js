(function () {

    function getToken(root) {
        const scoped = (root || document).querySelector('input[name="__RequestVerificationToken"]');
        if (scoped) return scoped.value;

        const global = document.querySelector('input[name="__RequestVerificationToken"]');
        return global ? global.value : "";
    }

    function initPlanning(root) {
        root = root || document;

        const backlogColumn = root.querySelector("#backlogColumn");
        const sprintColumn = root.querySelector("#sprintColumn");
        const capacityFill = root.querySelector("#capacityFill");
        const capacityText = root.querySelector("#capacityText");
        const sprintCount = root.querySelector("#sprintCount");
        const backlogCount = root.querySelector("#backlogCount");
        const capacityBar = root.querySelector("#capacityBar");

        if (!backlogColumn || !sprintColumn || typeof Sortable === "undefined") return;

        const sprintId = sprintColumn.dataset.sprintId;

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
            formData.append("__RequestVerificationToken", getToken(root));

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

        async function onAddToSprint(evt) {
            const storyId = evt.item.dataset.id;
            const ok = await postAction("/Sprint/AssignToSprint", storyId);

            if (!ok) {
                evt.from.insertBefore(evt.item, evt.from.children[evt.oldIndex] || null);
            }

            updateCapacityUI();
        }

        async function onAddToBacklog(evt) {
            const storyId = evt.item.dataset.id;
            const ok = await postAction("/Sprint/RemoveFromSprintPlanning", storyId);

            if (!ok) {
                evt.from.insertBefore(evt.item, evt.from.children[evt.oldIndex] || null);
            }

            updateCapacityUI();
        }

        Sortable.create(backlogColumn, {
            group: "planning",
            animation: 150,
            handle: ".planning-drag-handle",
            forceFallback: true,
            ghostClass: "planning-ghost",
            chosenClass: "planning-chosen",
            onAdd: onAddToBacklog,
            onSort: updateCapacityUI
        });

        Sortable.create(sprintColumn, {
            group: "planning",
            animation: 150,
            handle: ".planning-drag-handle",
            forceFallback: true,
            ghostClass: "planning-ghost",
            chosenClass: "planning-chosen",
            onAdd: onAddToSprint,
            onSort: updateCapacityUI
        });

        updateCapacityUI();
    }

    document.addEventListener("DOMContentLoaded", function () {
        initPlanning(document);
    });

    window.SmartTask = window.SmartTask || {};
    window.SmartTask.initPlanning = initPlanning;

})();