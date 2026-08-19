(function () {
    var input = document.getElementById("globalSearchInput");
    var resultsBox = document.getElementById("globalSearchResults");
    if (!input || !resultsBox) return;

    var debounceTimer = null;
    var currentController = null;

    var typeLabel = { Project: "پروژه", UserStory: "Story", Task: "Task", Sprint: "اسپرینت", Workspace: "فضای کاری", Label: "برچسب" };

    function render(results) {
        if (!results.length) {
            resultsBox.innerHTML = '<div class="search-empty">نتیجه‌ای پیدا نشد</div>';
            resultsBox.classList.add("show");
            return;
        }

        resultsBox.innerHTML = results.map(function (r) {
            return '' +
                '<a href="' + r.url + '" class="search-result-item">' +
                '  <span class="search-result-icon" style="background:' + r.color + '1A;color:' + r.color + ';">' +
                '    <i class="' + r.icon + '"></i>' +
                '  </span>' +
                '  <span class="search-result-text">' +
                '    <span class="search-result-title">' + r.title + '</span>' +
                '    <span class="search-result-sub">' + (typeLabel[r.type] || r.type) + (r.subTitle ? " · " + r.subTitle : "") + '</span>' +
                '  </span>' +
                '</a>';
        }).join("");

        resultsBox.classList.add("show");
    }

    input.addEventListener("input", function () {
        var q = input.value.trim();
        clearTimeout(debounceTimer);

        if (q.length < 2) {
            resultsBox.classList.remove("show");
            resultsBox.innerHTML = "";
            return;
        }

        debounceTimer = setTimeout(function () {
            if (currentController) currentController.abort();
            currentController = new AbortController();

            fetch("/Search/GlobalSearch?q=" + encodeURIComponent(q), {
                signal: currentController.signal,
                headers: { "X-Requested-With": "XMLHttpRequest" }
            })
                .then(function (res) { return res.json(); })
                .then(render)
                .catch(function (err) {
                    if (err.name !== "AbortError") console.error(err);
                });
        }, 300);
    });

    document.addEventListener("click", function (e) {
        if (!resultsBox.contains(e.target) && e.target !== input) {
            resultsBox.classList.remove("show");
        }
    });

    input.addEventListener("keydown", function (e) {
        if (e.key === "Escape") {
            resultsBox.classList.remove("show");
            input.blur();
        }
    });
})();