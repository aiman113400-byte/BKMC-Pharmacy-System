
(() => {
    "use strict";

    const THEME_KEY = "bkmc-theme";
    const root = document.documentElement;

    function getSavedTheme() {
        try {
            const savedTheme = localStorage.getItem(THEME_KEY);

            if (savedTheme === "light" || savedTheme === "dark") {
                return savedTheme;
            }
        } catch (error) {
            console.warn("Could not read saved theme preference.", error);
        }

        // First visit: Light Mode
        return "light";
    }

    function applyTheme(theme, savePreference = true) {
        const selectedTheme = theme === "dark" ? "dark" : "light";

        root.setAttribute("data-theme", selectedTheme);

        const toggleButton = document.getElementById("themeToggle");
        const toggleIcon = document.getElementById("themeToggleIcon");

        if (toggleIcon) {
            // Icon shows the theme the user can switch TO.
            toggleIcon.className = selectedTheme === "light"
                ? "bi bi-moon-stars"
                : "bi bi-sun";
        }

        if (toggleButton) {
            const nextTheme = selectedTheme === "light" ? "dark" : "light";
            const label = nextTheme === "dark"
                ? "Switch to dark mode"
                : "Switch to light mode";

            toggleButton.setAttribute("aria-label", label);
            toggleButton.setAttribute("title", label);
        }

        if (savePreference) {
            try {
                localStorage.setItem(THEME_KEY, selectedTheme);
            } catch (error) {
                console.warn("Could not save theme preference.", error);
            }
        }
    }

    function initializeTheme() {
        applyTheme(getSavedTheme(), false);

        const toggleButton = document.getElementById("themeToggle");

        if (!toggleButton || toggleButton.dataset.themeInitialized === "true") {
            return;
        }

        toggleButton.dataset.themeInitialized = "true";

        toggleButton.addEventListener("click", function (event) {
            event.preventDefault();

            const currentTheme = root.getAttribute("data-theme");
            const nextTheme = currentTheme === "light" ? "dark" : "light";

            applyTheme(nextTheme);
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initializeTheme);
    } else {
        initializeTheme();
    }
})();