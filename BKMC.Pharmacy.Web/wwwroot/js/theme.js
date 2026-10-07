/* ============================================================
   DARK MODE TOGGLE LOGIC
   File: wwwroot/js/theme.js
   ============================================================ */

(function () {
    'use strict';

    const STORAGE_KEY = 'theme';
    const LIGHT = 'light';
    const DARK = 'dark';

    // ================= GET CURRENT THEME =================
    function getCurrentTheme() {
        return document.documentElement.getAttribute('data-theme') || LIGHT;
    }

    // ================= APPLY THEME =================
    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        updateToggleIcon(theme);
    }

    // ================= UPDATE ICON (Moon/Sun) =================
    function updateToggleIcon(theme) {
        const icon = document.getElementById('themeIcon');
        if (!icon) return;

        if (theme === DARK) {
            icon.classList.remove('bi-moon');
            icon.classList.add('bi-sun');
        } else {
            icon.classList.remove('bi-sun');
            icon.classList.add('bi-moon');
        }
    }

    // ================= TOGGLE THEME =================
    function toggleTheme() {
        const current = getCurrentTheme();
        const next = current === DARK ? LIGHT : DARK;

        applyTheme(next);
        localStorage.setItem(STORAGE_KEY, next);
    }

    // ================= INITIALIZE =================
    document.addEventListener('DOMContentLoaded', function () {

        // Anti-flash script ne pehle hi set kar diya hoga, bas icon update karo
        const current = getCurrentTheme();
        updateToggleIcon(current);

        // Toggle button click
        const toggleBtn = document.getElementById('themeToggle');
        if (toggleBtn) {
            toggleBtn.addEventListener('click', toggleTheme);
        }
    });

    // Keyboard shortcut: Ctrl + Shift + D
    document.addEventListener('keydown', function (e) {
        if (e.ctrlKey && e.shiftKey && e.key.toLowerCase() === 'd') {
            e.preventDefault();
            toggleTheme();
        }
    });

})();