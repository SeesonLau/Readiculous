// User Master Screen JavaScript
// Handles all functionality for the UserMasterScreen.cshtml view

// Document ready wrapper
document.addEventListener('DOMContentLoaded', function () {
    // Initialize the UserMasterScreen module
    UserMasterScreen.init();
});

// UserMasterScreen module
const UserMasterScreen = (function () {
    // Private variables
    const loadingSpinner = `
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-2">Loading users...</p>
        </div>`;

    // Private methods
    function initializeEventHandlers() {
        // Search string input with debounce
        let filterTimeout;
        $('#searchString').on('input', function () {
            clearTimeout(filterTimeout);
            filterTimeout = setTimeout(loadFilteredResults, 300);
        });

        // Role filter change
        $('#roleType').on('change', loadFilteredResults);

        // Sort type change
        $('#searchType').on('change', function () {
            const searchType = $(this).val();
            updateUrlWithFilters(searchType);
        });

        // Modal handlers
        $('#addUserModal').on('show.bs.modal', function () {
            $('#addUserModalBody').load('@Url.Action("UserAddModal", "UserMaster")');
        });

        $(document).on('click', '.edit-user', function () {
            const userId = $(this).data('userid');
            $('#editUserModalBody').html(loadingSpinner);
            $('#editUserModalBody').load(`@Url.Action("UserEditModal", "UserMaster")?userId=${userId}`, function () {
                $('#editUserModal').modal('show');
            });
        });

        $(document).on('click', '.view-user', function () {
            const userId = $(this).data('userid');
            $('#viewUserModalBody').html(loadingSpinner);
            $('#viewUserModalBody').load(`@Url.Action("UserViewModal", "UserMaster")?userId=${userId}`, function () {
                $('#viewUserModal').modal('show');
            });
        });

        $(document).on('click', '.delete-user', function () {
            const userId = $(this).data('userid');
            const userName = $(this).data('username');
            $('#userToDeleteId').val(userId);
            $('#userToDeleteName').text(userName);
            $('#deleteUserModal').modal('show');
        });

        // AJAX form submissions
        $(document).on('submit', '#addUserForm, #editUserForm', handleFormSubmit);

        // Delete confirmation handler
        $('#confirmDeleteBtn').click(handleDeleteConfirmation);
    }

    function initializeFromUrl() {
        const urlParams = new URLSearchParams(window.location.search);
        ['searchString', 'roleType', 'searchType'].forEach(param => {
            if (urlParams.has(param)) {
                $(`#${param}`).val(urlParams.get(param));
            }
        });
    }

    function updateUrlWithFilters(searchType) {
        const searchString = $('#searchString').val().trim();
        const roleType = $('#roleType').val();

        let url = '@Url.Action("UserMasterScreen", "UserMaster")?';
        url += `searchType=${encodeURIComponent(searchType)}`;

        if (searchString) {
            url += `&searchString=${encodeURIComponent(searchString)}`;
        }
        if (roleType) {
            url += `&roleType=${encodeURIComponent(roleType)}`;
        }

        window.location.href = url;
    }

    async function loadFilteredResults() {
        const filters = {
            searchString: $('#searchString').val().trim(),
            roleType: $('#roleType').val(),
            searchType: $('#searchType').val()
        };

        $('#userListContainer').html(loadingSpinner);

        try {
            const response = await $.get('@Url.Action("UserMasterScreen", "UserMaster")', filters);
            $('#userListContainer').html($(response).find('#userListContainer').html());
            history.replaceState(null, '', `?${$.param(filters)}`);
        } catch (error) {
            console.error('Filter error:', error);
            showErrorAlert('Failed to load users. Please try again.');
        }
    }

    function handleFormSubmit(e) {
        e.preventDefault();
        const form = $(this);
        const submitBtn = form.find('button[type="submit"]');
        const originalBtnText = submitBtn.html();

        submitBtn.prop('disabled', true).html(`
            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            Processing...
        `);

        const formData = new FormData(form[0]);

        $.ajax({
            url: form.attr('action'),
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    window.location.reload();
                } else {
                    const modalBody = form.attr('id') === 'addUserForm'
                        ? '#addUserModalBody'
                        : '#editUserModalBody';
                    $(modalBody).html(response);
                }
            },
            error: function (xhr) {
                showErrorAlert(xhr.responseText || 'Request failed');
            },
            complete: function () {
                submitBtn.prop('disabled', false).html(originalBtnText);
            }
        });
    }

    function handleDeleteConfirmation() {
        const btn = $(this);
        const originalBtnText = btn.html();
        const userId = $('#userToDeleteId').val();

        btn.prop('disabled', true).html(`
            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            Deleting...
        `);

        $.post('@Url.Action("Delete", "UserMaster")', { userId: userId })
            .done(function () {
                window.location.reload();
            })
            .fail(function (xhr) {
                showErrorAlert(xhr.responseText || 'Delete failed');
            })
            .always(function () {
                btn.prop('disabled', false).html(originalBtnText);
                $('#deleteUserModal').modal('hide');
            });
    }

    function showErrorAlert(message) {
        $('#userListContainer').html(`
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle me-2"></i>
                ${message}
            </div>`);
    }

    // Public API
    return {
        init: function () {
            initializeFromUrl();
            initializeEventHandlers();
        }
    };
})();