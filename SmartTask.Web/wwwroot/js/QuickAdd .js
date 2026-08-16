(function () {
    var modalEl = document.getElementById("quickAddModal");
    if (!modalEl) return;

    var bsModal = new bootstrap.Modal(modalEl);
    var steps = {
        1: document.getElementById("qaStep1"),
        2: document.getElementById("qaStep2"),
        3: document.getElementById("qaStep3")
    };
    var stepIndicators = modalEl.querySelectorAll(".quick-add-steps .step");

    var selectedStoryId = null;
    var allProjects = [];
    var allStories = [];

    function getToken() {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
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

    function renderProjects(list) {
        var box = document.getElementById("qaProjectList");
        if (!list.length) {
            box.innerHTML = '<div class="qa-empty">هیچ پروژه‌ای در دسترس نیست</div>';
            return;
        }
        box.innerHTML = list.map(function (p) {
            return '' +
                '<button type="button" class="qa-project-item" data-id="' + p.id + '">' +
                '  <span class="qa-project-icon" style="background:' + p.color + '1A;color:' + p.color + ';"><i class="' + p.icon + '"></i></span>' +
                '  <span class="qa-project-text">' +
                '    <span class="qa-project-name">' + p.name + '</span>' +
                '    <span class="qa-project-workspace">' + p.workspaceName + '</span>' +
                '  </span>' +
                '</button>';
        }).join("");

        box.querySelectorAll(".qa-project-item").forEach(function (btn) {
            btn.addEventListener("click", function () {
                loadStories(Number(btn.dataset.id));
                goToStep(2);
            });
        });
    }

    function renderStories(list) {
        var box = document.getElementById("qaStoryList");
        if (!list.length) {
            box.innerHTML = '<div class="qa-empty">این پروژه User Story‌ای ندارد</div>';
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
                goToStep(3);
                document.getElementById("qaTaskTitle").focus();
            });
        });
    }

    function loadProjects() {
        document.getElementById("qaProjectList").innerHTML = '<div class="qa-loading">در حال بارگذاری...</div>';
        fetch("/Search/ProjectsForQuickAdd")
            .then(function (res) { return res.json(); })
            .then(function (list) { allProjects = list; renderProjects(list); });
    }

    function loadStories(projectId) {
        document.getElementById("qaStoryList").innerHTML = '<div class="qa-loading">در حال بارگذاری...</div>';
        fetch("/Search/StoriesForQuickAdd?projectId=" + projectId)
            .then(function (res) { return res.json(); })
            .then(function (list) { allStories = list; renderStories(list); });
    }

    function resetModal() {
        selectedStoryId = null;
        document.getElementById("qaTaskForm").reset();
        goToStep(1);
    }

    function openModal() {
        resetModal();
        loadProjects();
        bsModal.show();
    }

    document.querySelectorAll("[data-quick-add-open]").forEach(function (btn) {
        btn.addEventListener("click", openModal);
    });

    modalEl.querySelectorAll(".qa-back-btn").forEach(function (btn) {
        btn.addEventListener("click", function () {
            goToStep(Number(btn.dataset.back));
        });
    });

    var projectFilter = document.getElementById("qaProjectFilter");
    if (projectFilter) {
        projectFilter.addEventListener("input", function () {
            var q = this.value.trim().toLowerCase();
            renderProjects(allProjects.filter(function (p) { return p.name.toLowerCase().indexOf(q) !== -1; }));
        });
    }

    var storyFilter = document.getElementById("qaStoryFilter");
    if (storyFilter) {
        storyFilter.addEventListener("input", function () {
            var q = this.value.trim().toLowerCase();
            renderStories(allStories.filter(function (s) { return s.title.toLowerCase().indexOf(q) !== -1; }));
        });
    }

    document.getElementById("qaTaskForm").addEventListener("submit", function (e) {
        e.preventDefault();
        var title = document.getElementById("qaTaskTitle").value.trim();
        var priority = document.getElementById("qaTaskPriority").value;
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
                    if (data.url) {
                        setTimeout(function () { window.location.href = data.url; }, 900);
                    }
                } else {
                    showError(data.message || "خطا در ساخت Task.");
                }
            })
            .catch(function () {
                submitBtn.disabled = false;
                showError("خطا در ارتباط با سرور.");
            });
    });

    document.addEventListener("keydown", function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "k") {
            e.preventDefault();
            openModal();
        }
    });
})();