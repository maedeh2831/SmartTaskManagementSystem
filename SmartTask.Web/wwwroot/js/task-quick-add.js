(function () {
    var modalEl = document.getElementById("taskQuickAddModal");
    if (!modalEl) return;

    var bsModal = new bootstrap.Modal(modalEl);
    var steps = {
        1: document.getElementById("tqaStep1"),
        2: document.getElementById("tqaStep2")
    };
    var stepIndicators = modalEl.querySelectorAll(".quick-add-steps .step");

    var selectedStoryId = null;
    var allStories = [];
    var currentProjectId = null;

    function getToken() {
        var el = document.querySelector('#antiForgeryForm input[name="__RequestVerificationToken"]')
            || document.querySelector('#globalAntiForgeryForm input[name="__RequestVerificationToken"]');
        return el ? el.value : "";
    }

    function goToStep(n) {
        Object.keys(steps).forEach(function (k) {
            steps[k].classList.toggle("d-none", Number(k) !== n);
        });
        stepIndicators.forEach(function (s) {
            s.classList.toggle("active", Number(s.dataset.step) <= n);
        });
    }

    function renderStories(list) {
        var box = document.getElementById("tqaStoryList");
        if (!list.length) {
            box.innerHTML = '<div class="qa-empty">این پروژه User Story‌ای ندارد. اول از بک‌لاگ یک User Story بسازید.</div>';
            return;
        }
        box.innerHTML = list.map(function (s) {
            return '' +
                '<button type="button" class="qa-story-item" data-id="' + s.id + '">' +
                '  <i class="fa-solid fa-bookmark"></i>' +
                '  <span>' + s.title + '</span>' +
                '</button>';
        }).join("");

        box.querySelectorAll(".qa-story-item").forEach(function (btn) {
            btn.addEventListener("click", function () {
                selectedStoryId = Number(btn.dataset.id);
                goToStep(2);
                document.getElementById("tqaTaskTitle").focus();
            });
        });
    }

    function loadStories() {
        document.getElementById("tqaStoryList").innerHTML = '<div class="qa-loading">در حال بارگذاری...</div>';
        fetch("/Search/StoriesForQuickAdd?projectId=" + currentProjectId)
            .then(function (res) { return res.json(); })
            .then(function (list) { allStories = list; renderStories(list); });
    }

    function resetModal() {
        selectedStoryId = null;
        document.getElementById("tqaTaskForm").reset();
        goToStep(1);
    }

    function openModal(projectId) {
        currentProjectId = projectId;
        resetModal();
        loadStories();
        bsModal.show();
    }

    document.querySelectorAll("[data-task-quick-add-open]").forEach(function (btn) {
        btn.addEventListener("click", function () {
            openModal(btn.dataset.projectId);
        });
    });

    modalEl.querySelectorAll(".qa-back-btn").forEach(function (btn) {
        btn.addEventListener("click", function () {
            goToStep(Number(btn.dataset.back));
        });
    });

    var storyFilter = document.getElementById("tqaStoryFilter");
    if (storyFilter) {
        storyFilter.addEventListener("input", function () {
            var q = this.value.trim().toLowerCase();
            renderStories(allStories.filter(function (s) { return s.title.toLowerCase().indexOf(q) !== -1; }));
        });
    }

    document.getElementById("tqaTaskForm").addEventListener("submit", function (e) {
        e.preventDefault();
        var title = document.getElementById("tqaTaskTitle").value.trim();
        var priority = document.getElementById("tqaTaskPriority").value;
        if (!title || !selectedStoryId) return;

        var submitBtn = this.querySelector(".qa-submit-btn");
        submitBtn.disabled = true;

        var formData = new URLSearchParams();
        formData.append("userStoryId", selectedStoryId);
        formData.append("title", title);
        formData.append("priority", priority);
        formData.append("__RequestVerificationToken", getToken());

        fetch("/Task/QuickCreate", {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: formData.toString()
        })
            .then(function (res) { return res.json(); })
            .then(function (data) {
                submitBtn.disabled = false;
                if (data.success) {
                    bsModal.hide();
                    showSuccess("Task با موفقیت ساخته شد.");
                    setTimeout(function () { window.location.reload(); }, 900);
                } else {
                    showError(data.message || "خطا در ساخت Task.");
                }
            })
            .catch(function () {
                submitBtn.disabled = false;
                showError("خطا در ارتباط با سرور.");
            });
    });
})();