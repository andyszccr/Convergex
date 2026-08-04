(() => {
    const frame = document.getElementById("appFrame");
    const toggle = document.getElementById("sidebarToggle");
    const backdrop = document.getElementById("sidebarBackdrop");

    if (!frame || !toggle) {
        return;
    }

    const closeSidebar = () => frame.classList.remove("sidebar-open");
    const toggleSidebar = () => frame.classList.toggle("sidebar-open");

    toggle.addEventListener("click", toggleSidebar);
    backdrop?.addEventListener("click", closeSidebar);

    window.addEventListener("resize", () => {
        if (window.innerWidth >= 992) {
            closeSidebar();
        }
    });
})();
