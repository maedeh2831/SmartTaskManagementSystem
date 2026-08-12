document.addEventListener("DOMContentLoaded", function () {
    if (typeof $ === "undefined" || !$.fn.persianDatepicker) return;

    document.querySelectorAll(".date-picker-display").forEach(function (input) {
        var $input = $(input);
        var includeTime = input.dataset.includeTime === "true";
        var $hidden = $(input.dataset.hiddenTarget);

        $input.persianDatepicker({
            format: includeTime ? "YYYY/MM/DD HH:mm" : "YYYY/MM/DD",
            autoClose: true,
            timePicker: {
                enabled: includeTime
            },
            onSelect: function (unixMs) {
                var d = new Date(unixMs);
                var pad = function (n) { return n < 10 ? "0" + n : "" + n; };
                var iso = d.getFullYear() + "-" + pad(d.getMonth() + 1) + "-" + pad(d.getDate()) +
                    "T" + pad(d.getHours()) + ":" + pad(d.getMinutes()) + ":00";
                $hidden.val(iso);
            }
        });
    });
});