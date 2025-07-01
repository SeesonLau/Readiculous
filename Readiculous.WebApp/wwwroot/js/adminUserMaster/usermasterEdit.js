const UserEditModal = (function () {
    function initializeEventHandlers() {
        const removeCheckbox = document.getElementById('removeProfilePicture');
        const fileInput = document.getElementById('newProfilePicture');

        if (removeCheckbox && fileInput) {
            // When a new file is selected, uncheck the remove checkbox
            fileInput.addEventListener('change', function () {
                if (this.files.length > 0) {
                    removeCheckbox.checked = false;
                }
            });

            // When remove is checked, clear the file input
            removeCheckbox.addEventListener('change', function () {
                if (this.checked) {
                    fileInput.value = '';
                }
            });
        }
    }

    return {
        init: function () {
            initializeEventHandlers();
        }
    };
})();

document.addEventListener('DOMContentLoaded', function () {
    UserEditModal.init();
});