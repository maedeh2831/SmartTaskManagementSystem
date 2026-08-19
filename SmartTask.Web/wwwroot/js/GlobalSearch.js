(function () {
    var input = document.getElementById("globalSearchInput");
    var resultsBox = document.getElementById("globalSearchResults");
    if (!input || !resultsBox) return;

    var debounceTimer = null;
    var currentController = null;
    var selectedIndex = -1;

    var typeLabel = { Project: "پروژه", UserStory: "Story", Task: "Task", Sprint: "اسپرینت", Workspace: "فضای کاری", Label: "برچسب" };

    function render(results) {
        resultsBox.classList.remove("loading");
        var searchContainer = input.closest(".header-search");
        if (searchContainer) searchContainer.classList.remove("searching");

        if (!results.length) {
            resultsBox.innerHTML = '<div class="search-empty"><i class="fa-solid fa-magnifying-glass" style="font-size:18px;margin-bottom:6px;display:block;color:var(--gray300);"></i>نتیجه‌ای پیدا نشد</div>';
            resultsBox.classList.add("show");
            selectedIndex = -1;
            return;
        }

        resultsBox.innerHTML = results.map(function (r, i) {
            return '' +
                '<a href="' + r.url + '" class="search-result-item" data-index="' + i + '">' +
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
        selectedIndex = -1;
    }

    function showLoading() {
        resultsBox.innerHTML = '<div class="search-loading">در حال جستجو...</div>';
        resultsBox.classList.add("show", "loading");
        var searchContainer = input.closest(".header-search");
        if (searchContainer) searchContainer.classList.add("searching");
    }

    function navigateResults(direction) {
        var items = resultsBox.querySelectorAll(".search-result-item");
        if (!items.length) return;

        items.forEach(function (item) { item.classList.remove("focused"); });

        if (direction === "down") {
            selectedIndex = Math.min(selectedIndex + 1, items.length - 1);
        } else {
            selectedIndex = Math.max(selectedIndex - 1, 0);
        }

        items[selectedIndex].classList.add("focused");
        items[selectedIndex].scrollIntoView({ block: "nearest" });
    }

    input.addEventListener("input", function () {
        var q = input.value.trim();
        clearTimeout(debounceTimer);
        selectedIndex = -1;

        if (q.length < 2) {
            resultsBox.classList.remove("show", "loading");
            resultsBox.innerHTML = "";
            var searchContainer = input.closest(".header-search");
            if (searchContainer) searchContainer.classList.remove("searching");
            return;
        }

        showLoading();

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
                    if (err.name !== "AbortError") {
                        resultsBox.classList.remove("loading");
                        var sc = input.closest(".header-search");
                        if (sc) sc.classList.remove("searching");
                        console.error(err);
                    }
                });
        }, 300);
    });

    input.addEventListener("keydown", function (e) {
        if (e.key === "Escape") {
            resultsBox.classList.remove("show");
            input.blur();
            return;
        }

        var items = resultsBox.querySelectorAll(".search-result-item");
        if (!items.length) return;

        if (e.key === "ArrowDown") {
            e.preventDefault();
            navigateResults("down");
        } else if (e.key === "ArrowUp") {
            e.preventDefault();
            navigateResults("up");
        } else if (e.key === "Enter" && selectedIndex >= 0) {
            e.preventDefault();
            items[selectedIndex].click();
        }
    });

    document.addEventListener("click", function (e) {
        if (!resultsBox.contains(e.target) && e.target !== input) {
            resultsBox.classList.remove("show");
        }
    });
})();