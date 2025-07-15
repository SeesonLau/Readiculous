function setupLogoutConfirmation(logoutUrl) {
    const modal = document.querySelector('.logout-confirmation-modal');
    const confirmBtn = document.getElementById('confirmLogout');
    const cancelBtn = document.getElementById('cancelLogout');

    function showModal() {
        modal.style.display = 'flex';
    }

    function hideModal() {
        modal.style.display = 'none';
    }

    confirmBtn.addEventListener('click', () => {
        window.location.href = logoutUrl;
    });

    cancelBtn.addEventListener('click', hideModal);

    // Close when clicking outside the modal
    modal.addEventListener('click', (e) => {
        if (e.target === modal) {
            hideModal();
        }
    });

    return {
        showModal
    };
}

