const UserMasterScreen = (function () {
    let settings = {};
    const loadingSpinner = `
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-2">Loading users...</p>
        </div>`;
    function initializeEventHandlers() {
        // Search string input with debounce
        let filterTimeout;
        $('#searchString').on('input', function () {
            clearTimeout(filterTimeout);
            filterTimeout = setTimeout(loadFilteredResults, 300);
        });
        // Role filter 
        $('#roleType').on('change', loadFilteredResults);
        // Sort type 
        $('#searchType').on('change', function () {
            const searchType = $(this).val();
            updateUrlWithFilters(searchType);
        });

        // Add User Modal
        $('#addUserModal').on('show.bs.modal', function () {
            $('#addUserModalBody').load(settings.userAddModalUrl);
        });
        // Edit User Modal
        $(document).on('click', '.edit-user', function () {
            const userId = $(this).data('userid');
            $('#editUserModalBody').html(loadingSpinner);
            $('#editUserModalBody').load(`${settings.userEditModalUrl}?userId=${userId}`, function () {
                $('#editUserModal').modal('show');
            });
        });
        //View User Modal
        $(document).on('click', '.view-user', function () {
            const userId = $(this).data('userid');
            $('#viewUserModalBody').html(loadingSpinner);
            $('#viewUserModalBody').load(`${settings.userViewModalUrl}?userId=${userId}`, function () {
                $('#viewUserModal').modal('show');
            });
        });
        //Delete User Modal
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

        let url = `${settings.userMasterScreenUrl}?`;
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
            const response = await $.get(settings.userMasterScreenUrl, filters);
            $('#userListContainer').html($(response).find('#userListContainer').html());
            history.replaceState(null, '', `?${$.param(filters)}`);
        } catch (error) {
            console.error('Filter error:', error);
            showErrorAlert('Failed to load users. Please try again.');
        }
    }
    // Submit Button
    function handleFormSubmit(e) {
        e.preventDefault();
        const form = $(this);
        const submitBtn = form.find('button[type="submit"]');
        const originalBtnText = submitBtn.html();

        // Check if form has all fields filled out
        if (!form.valid()) {
            showErrorAlert('Please fill in all required fields correctly.');
            return;
        }

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

        $.post(settings.deleteUserUrl, { userId: userId })
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
    // Alert for errors
    function showErrorAlert(message) {
        $('#userListContainer').html(`
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle me-2"></i>
                ${message}
            </div>`);
    }

    // Public API
    return {
        init: function (config) {
            settings = config;
            initializeFromUrl();
            initializeEventHandlers();
        }
    };
})();

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    UserMasterScreen.init(userMasterSettings);
});


/* 
Current Issues:

1.Latest is supposed to be Updated Time , not Created Time. STATUS: DONE
2. SortBy Issues:

    Case 1:
        Search Username/Email = NULL
        Filter Role = All Role
            Sort By: wont work when switcing sorts
            Probable cause: Having Latest as the initial rendering.

    Case2:
        Search Username/Email = with inputs
        Filter Role = All Role
            Sort By: works

    Case3:
        Search Username/Email = with inputs
        Filter Role = either Admin/Reviewer
            Sort By: works

    Case4:
        Search Username/Email = NULL
        Filter Role = either Admin/Reviewer
            Sort By: works
3. UserAddModal
    After successfully adding the user, the user automatically gets inputted in the list. 
        a. Issue: loads too long.
       

4. UserEditModal
    After successfully editing the user, the user automatically gets inputted in the list. 
        a. Issue: Password and ProfilePicture needs inputs: STATUS: DONE
        b. Issue: Upload picture then submit takes too long. Need a way to automatically crop the image.
        c. Issue: Remove Profile Picture not yet working: STATUS: DONE

5. UserDeleteModal
    After successfully editing the user, the user automatically gets delete from the list. 
        Issue: None

6. UserViewDetails
        Issue: Profile picture is not showing up

7. User Add Feature
    Currently lack a logic to generate userId and password for the user.
*/