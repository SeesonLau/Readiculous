document.addEventListener("DOMContentLoaded", () => {
    const authModal = document.getElementById("authModal");

    function showModal(modalId) {
        // Hide all auth boxes
        document.querySelectorAll(".auth-box").forEach(box => {
            box.classList.add("hidden");
        });

        // Show the selected one
        const target = document.querySelector(`.auth-box[data-id="${modalId}"]`);
        if (target) {
            target.classList.remove("hidden");
            authModal.classList.remove("hidden");
            authModal.classList.add("show");

            // Reset forms when showing modals
            if (modalId === "login" && window.resetLoginForm) {
                window.resetLoginForm();
            }
            if (modalId === "register" && window.resetRegisterForm) {
                window.resetRegisterForm();
            }
        }
    }

    function closeModal() {
        authModal.classList.remove("show");
        authModal.classList.add("hidden");
    }

    // Make functions globally accessible
    window.showModal = showModal;
    window.closeModal = closeModal;

    // Show modal if passed from server
    const modalFromServer = authModal?.dataset?.modal;
    if (modalFromServer) {
        showModal(modalFromServer);
    }
});
