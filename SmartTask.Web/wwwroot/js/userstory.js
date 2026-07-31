document.addEventListener("DOMContentLoaded", function () {

    // ===== Live Search (Index) =====
    const searchInput = document.getElementById("storySearchInput");
    const list = document.getElementById("storyList");

    if (searchInput && list) {
        searchInput.addEventListener("input", function () {
            const term = searchInput.value.trim().toLowerCase();
            list.querySelectorAll(".backlog-row").forEach(row => {
                const title = (row.dataset.title || "").toLowerCase();
                row.style.display = title.includes(term) ? "" : "none";
            });
        });
    }

    // ===== Live Preview (Create) =====
    const titleInput = document.getElementById("storyTitle");
    const descInput = document.getElementById("storyDescription");
    const pointInput = document.getElementById("storyPoint");

    const previewTitle = document.getElementById("previewTitle");
    const previewDescription = document.getElementById("previewDescription");
    const previewPoints = document.getElementById("previewPoints");

    if (titleInput && previewTitle) {
        titleInput.addEventListener("input", () => {
            previewTitle.innerText = titleInput.value || "User Story جدید";
        });
    }

    if (descInput && previewDescription) {
        descInput.addEventListener("input", () => {
            previewDescription.innerText = descInput.value || "توضیحات این Story اینجا نمایش داده خواهد شد...";
        });
    }

    if (pointInput && previewPoints) {
        pointInput.addEventListener("input", () => {
            previewPoints.innerText = `${pointInput.value || 0} Story Point`;
        });
    }

});