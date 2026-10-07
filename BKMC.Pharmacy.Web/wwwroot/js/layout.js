document.addEventListener("DOMContentLoaded", function () {

    const sidebar = document.getElementById("sidebar");
    const sidebarToggle = document.getElementById("sidebarToggle");
    const sidebarClose = document.getElementById("sidebarClose");
    const sidebarOverlay = document.getElementById("sidebarOverlay");

    function openSidebar() {
        sidebar.classList.add("sidebar-open");
        sidebarOverlay.classList.add("active");
        document.body.classList.add("sidebar-is-open");
    }

    function closeSidebar() {
        sidebar.classList.remove("sidebar-open");
        sidebarOverlay.classList.remove("active");
        document.body.classList.remove("sidebar-is-open");
    }

    if (sidebarToggle) {
        sidebarToggle.addEventListener("click", openSidebar);
    }

    if (sidebarClose) {
        sidebarClose.addEventListener("click", closeSidebar);
    }

    if (sidebarOverlay) {
        sidebarOverlay.addEventListener("click", closeSidebar);
    }

    document.addEventListener("keydown", function (event) {

        if (event.key === "Escape") {
            closeSidebar();
        }

    });


    /*
     * Prevent placeholder navigation
     * until the corresponding Razor Page
     * is implemented.
     */
    document.querySelectorAll(".nav-placeholder")
        .forEach(function (item) {

            item.addEventListener("click", function (event) {

                event.preventDefault();

            });

        });

});