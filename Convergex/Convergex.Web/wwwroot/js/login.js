(() => {
    document.querySelectorAll(".toggle-password").forEach((button) => {
        button.addEventListener("click", () => {
            const targetId = button.getAttribute("data-target");
            const input = targetId ? document.getElementById(targetId) : null;
            if (!input) {
                return;
            }

            const hidden = input.getAttribute("type") === "password";
            input.setAttribute("type", hidden ? "text" : "password");
            button.innerHTML = hidden
                ? '<i class="bi bi-eye-slash"></i>'
                : '<i class="bi bi-eye"></i>';
        });
    });
})();
