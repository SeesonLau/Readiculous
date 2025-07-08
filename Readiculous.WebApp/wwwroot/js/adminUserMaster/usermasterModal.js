// User Edit Modal
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

// User Add Modal & User Edit Modal
function togglePassword(inputId, button) {
    const input = document.getElementById(inputId);
    const icon = button.querySelector('i');

    // Toggle input type
    input.type = input.type === 'password' ? 'text' : 'password';

    // Toggle icon classes
    icon.classList.toggle('fa-eye-slash');
    icon.classList.toggle('fa-eye');

    // Focus back on input for better UX
    input.focus();

    // Update aria-pressed state for accessibility
    button.setAttribute('aria-pressed', input.type === 'text');
}

// User View Modal
$(function () {
    $('.edit-user').click(function () {
        var userId = $(this).data('userid');
        $('#viewUserModal').modal('hide');

        $('#editUserModalBody').load('@Url.Action("UserEditModal", "UserMaster")?userId=' + userId, function () {
            $('#editUserModal').modal('show');
        });
    });
});