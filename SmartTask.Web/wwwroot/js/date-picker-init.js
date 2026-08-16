document.addEventListener("DOMContentLoaded", function () {
    if (
        typeof $ === "undefined" ||
        !$.fn.persianDatepicker ||
        typeof persianDate === "undefined"
    ) {
        return;
    }

    document.querySelectorAll(".date-picker-display").forEach(function (input) {
        var $input = $(input);

        // Prevent double initialization
        if ($input.data("pdp-initialized")) {
            return;
        }

        $input.data("pdp-initialized", true);

        var includeTime = input.dataset.includeTime === "true";
        var $hidden = $(input.dataset.hiddenTarget);

        var format = includeTime
            ? "YYYY/MM/DD HH:mm"
            : "YYYY/MM/DD";

        /*
         * Hidden value:
         * 2026-08-29T00:00:00
         *
         * Convert Gregorian -> Persian for display.
         */
        if ($hidden.length && $hidden.val()) {
            var gDate = new Date($hidden.val());

            if (!isNaN(gDate.getTime())) {
                var pd = new persianDate(gDate);

                $input.val(pd.format(format));
            }
        }

        /*
         * IMPORTANT:
         *
         * The value inside the visible input is Persian/Jalali,
         * therefore initialValueType MUST be "persian".
         */
        $input.persianDatepicker({
            format: format,

            calendarType: "persian",

            initialValue: true,
            initialValueType: "persian",

            persianDigit: false,

            autoClose: true,

            timePicker: {
                enabled: includeTime
            },

            onSelect: function (unixMs) {
                /*
                 * unixMs is Gregorian Unix timestamp.
                 */
                var d = new Date(unixMs);

                if (isNaN(d.getTime())) {
                    return;
                }

                /*
                 * Store Gregorian value in hidden input.
                 *
                 * Example:
                 * 2026-08-29T00:00:00
                 */
                var pad = function (n) {
                    return n < 10 ? "0" + n : "" + n;
                };

                var iso =
                    d.getFullYear() +
                    "-" +
                    pad(d.getMonth() + 1) +
                    "-" +
                    pad(d.getDate()) +
                    "T" +
                    pad(d.getHours()) +
                    ":" +
                    pad(d.getMinutes()) +
                    ":00";

                $hidden.val(iso);

                /*
                 * Keep visible input Jalali.
                 */
                var pdSelected = new persianDate(d);

                $input.val(pdSelected.format(format));

                console.log(
                    "Selected Jalali:",
                    pdSelected.format(format)
                );

                console.log(
                    "Hidden Gregorian:",
                    iso
                );
            }
        });
    });
});