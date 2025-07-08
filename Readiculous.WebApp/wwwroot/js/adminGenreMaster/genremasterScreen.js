const GenreMasterScreen = (function () {
    let settings = {};
    const loadingSpinner = `
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-2">Loading...</p>
        </div>`;

    function debounce(func, wait) {
        let timeout;
        return function () {
            const context = this, args = arguments;
            clearTimeout(timeout);
            timeout = setTimeout(() => {
                func.apply(context, args);
            }, wait);
        };
    }

    function initializeEventHandlers() {
        // View Modal
        document.addEventListener('click', function (e) {
            if (e.target.closest('.view-genre-btn')) {
                e.preventDefault();
                const genreId = e.target.closest('.view-genre-btn').dataset.genreId;
                openGenreViewModal(genreId);
            }
        });
        // Main screen pagination handlers
        $(document).on('click', '[data-page-size]', function (e) {
            e.preventDefault();
            const pageSize = $(this).data('page-size');
            updateUrlWithPageSize(pageSize);
        });

        $(document).on('click', '.pagination-container .page-link:not(.jump-to-page)', function (e) {
            e.preventDefault();
            const page = $(this).data('page');
            if (page && !$(this).closest('.page-item').hasClass('disabled')) {
                updateUrlWithPage(page);
            }
        });

        // Main screen jump to page handler
        $(document).on('click', '#confirmJumpToPage', function () {
            const pageNumber = parseInt($('#pageNumberInput').val());
            const totalPages = parseInt($('#pageNumberInput').attr('max'));

            if (isNaN(pageNumber)) {
                $('#pageNumberInput').addClass('is-invalid');
                $('#pageNumberError').text('Please enter a valid page number');
                return;
            }

            if (pageNumber < 1 || pageNumber > totalPages) {
                $('#pageNumberInput').addClass('is-invalid');
                $('#pageNumberError').text(`Please enter a page number between 1 and ${totalPages}`);
                return;
            }

            $('#pageNumberInput').removeClass('is-invalid');
            $('#pageNumberError').text('');
            $('#jumpToPageModal').modal('hide');
            updateUrlWithPage(pageNumber);
        });

        // Search and filter handlers
        $(document).on('input', 'input[name="searchString"]', debounce(loadFilteredResults, 300));
        $(document).on('change', 'select[name="searchType"]', loadFilteredResults);

        // Modal handlers
        $(document).on('click', '#openAddGenreModalBtn', loadAddGenreModal);
        $(document).on('click', '.edit-genre-btn', function (e) {
            e.preventDefault();
            const genreId = $(this).data('genre-id');
            loadEditGenreModal(genreId);
        });

        // Delete handlers
        $(document).on('click', '.btn-delete-genre', function () {
            const genreId = $(this).data('genre-id');
            const genreName = $(this).data('genre-name');
            $('#userToDeleteId').val(genreId);
            $('#userToDeleteName').text(genreName);
            $('#deleteUserModal').modal('show');
        });

        $(document).on('click', '#cancelDeleteGenreBtn', function () {
            $('#deleteUserModal').modal('hide');
        });

        $(document).on('click', '#confirmDeleteBtn', handleDeleteConfirmation);
    }

    function loadAddGenreModal() {
        $('#addGenreModalBody').html(loadingSpinner);
        $('#addGenreModal').modal('show');

        $.get(settings.genreAddModalUrl)
            .done(function (data) {
                $('#addGenreModalBody').html(data);
                initializeAddGenreForm();
            })
            .fail(function () {
                $('#addGenreModalBody').html('<div class="alert alert-danger">Failed to load add genre form</div>');
            });
    }

    function loadEditGenreModal(genreId) {
        $('#editGenreModalBody').html(loadingSpinner);
        $('#editGenreModal').modal('show');

        $.get(settings.genreEditModalUrl, { id: genreId })
            .done(function (data) {
                $('#editGenreModalBody').html(data);
                initializeEditGenreForm();
            })
            .fail(function () {
                $('#editGenreModalBody').html('<div class="alert alert-danger">Failed to load edit form</div>');
            });
    }
    function initializeAddGenreForm() {
        $('#addGenreForm').off('submit').on('submit', function (e) {
            e.preventDefault();
            const form = $(this);
            const submitBtn = form.find('button[type="submit"]');
            const originalBtnText = submitBtn.html();

            submitBtn.prop('disabled', true).html(`
                <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                Saving...
            `);

            // Clear previous errors
            $('#addGenreValidationSummary').text('');
            $('#addGenreNameError').text('');
            $('#addGenreDescriptionError').text('');

            $.ajax({
                url: settings.createGenreUrl,
                type: 'POST',
                data: form.serialize(),
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                success: function (response) {
                    if (response.success) {
                        $('#addGenreModal').modal('hide');
                        loadFilteredResults();
                    } else {
                        if (response.errors) {
                            Object.keys(response.errors).forEach(key => {
                                $(`#addGenre${key}Error`).text(response.errors[key][0]);
                            });
                        } else if (response.message) {
                            $('#addGenreValidationSummary').text(response.message);
                        }
                    }
                },
                error: function (xhr) {
                    if (xhr.responseJSON && xhr.responseJSON.errors) {
                        Object.keys(xhr.responseJSON.errors).forEach(key => {
                            $(`#addGenre${key}Error`).text(xhr.responseJSON.errors[key][0]);
                        });
                    } else {
                        $('#addGenreValidationSummary').text('An error occurred while saving.');
                    }
                },
                complete: function () {
                    submitBtn.prop('disabled', false).html(originalBtnText);
                }
            });
        });
    }
    function initializeEditGenreForm() {
        $('#editGenreForm').off('submit').on('submit', function (e) {
            e.preventDefault();
            const form = $(this);
            const submitBtn = form.find('button[type="submit"]');
            const originalBtnText = submitBtn.html();

            submitBtn.prop('disabled', true).html(`
                <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                Saving...
            `);

            // Clear previous errors
            $('#editGenreValidationSummary').text('');
            $('#editGenreNameError').text('');
            $('#editGenreDescriptionError').text('');

            $.ajax({
                url: settings.editGenreUrl,
                type: 'POST',
                data: form.serialize(),
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                success: function (response) {
                    if (response.success) {
                        $('#editGenreModal').modal('hide');
                        loadFilteredResults();
                    } else {
                        if (response.errors) {
                            Object.keys(response.errors).forEach(key => {
                                $(`#editGenre${key}Error`).text(response.errors[key][0]);
                            });
                        } else if (response.message) {
                            $('#editGenreValidationSummary').text(response.message);
                        }
                    }
                },
                error: function (xhr) {
                    if (xhr.responseJSON && xhr.responseJSON.errors) {
                        Object.keys(xhr.responseJSON.errors).forEach(key => {
                            $(`#editGenre${key}Error`).text(xhr.responseJSON.errors[key][0]);
                        });
                    } else {
                        $('#editGenreValidationSummary').text('An error occurred while saving.');
                    }
                },
                complete: function () {
                    submitBtn.prop('disabled', false).html(originalBtnText);
                }
            });
        });
    }

    function updateUrlWithPage(page) {
        const url = new URL(window.location.href);
        url.searchParams.set('page', page);
        window.history.replaceState({ path: url.toString() }, '', url.toString());
        loadFilteredResults();
    }

    function updateUrlWithPageSize(pageSize) {
        const url = new URL(window.location.href);
        url.searchParams.set('pageSize', pageSize);
        url.searchParams.set('page', '1');
        window.history.replaceState({ path: url.toString() }, '', url.toString());
        loadFilteredResults();
    }

    async function loadFilteredResults() {
        const form = $('form[method="get"]');
        const urlParams = new URLSearchParams(window.location.search);
        const page = urlParams.get('page') || '1';
        const pageSize = urlParams.get('pageSize') || '10';

        const formData = form.serialize() + `&page=${page}&pageSize=${pageSize}`;

        $('#genreListContainer').html(loadingSpinner);

        try {
            const response = await $.ajax({
                url: settings.genreMasterScreenUrl,
                type: 'GET',
                data: formData,
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            const content = $(response).find('#genreListContainer').html() || response;
            $('#genreListContainer').html(content);

            // Update pagination controls
            const paginationHtml = $(response).find('.pagination-container').html();
            if (paginationHtml) {
                $('.pagination-container').html(paginationHtml);
            }

            // Update page size controls
            const pageSizeHtml = $(response).find('.page-size-container').html();
            if (pageSizeHtml) {
                $('.page-size-container').html(pageSizeHtml);
            }
        } catch (error) {
            showErrorAlert('Failed to load genres. Please try again.');
        }
    }

    function handleDeleteConfirmation() {
        const submitBtn = $(this);
        const originalBtnText = submitBtn.html();
        const genreId = $('#userToDeleteId').val();

        submitBtn.prop('disabled', true).html(`
            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            Deleting...
        `);

        $.ajax({
            url: settings.deleteGenreUrl,
            type: 'POST',
            data: { id: genreId },
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            success: function (result) {
                if (result.success) {
                    $('#deleteUserModal').modal('hide');
                    loadFilteredResults();
                } else {
                    showErrorAlert('Error deleting genre: ' + (result.message || 'Unknown error'));
                }
            },
            error: function (xhr) {
                showErrorAlert('An error occurred while deleting the genre: ' + (xhr.responseJSON?.message || xhr.statusText));
            },
            complete: function () {
                submitBtn.prop('disabled', false).html(originalBtnText);
            }
        });
    }

    function showErrorAlert(message) {
        const alertHtml = `
            <div class="alert alert-danger alert-dismissible fade show" role="alert">
                <i class="fas fa-exclamation-triangle me-2"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>`;

        $('.container').prepend(alertHtml);
        setTimeout(() => $('.alert').alert('close'), 5000);
    }

    function openGenreViewModal(genreId) {
        fetch(`${genreMasterSettings.genreViewModalUrl}?id=${genreId}`)
            .then(response => response.text())
            .then(html => {
                // Create modal container if it doesn't exist
                let modalContainer = document.getElementById('genreViewModalContainer');
                if (!modalContainer) {
                    modalContainer = document.createElement('div');
                    modalContainer.id = 'genreViewModalContainer';
                    document.body.appendChild(modalContainer);
                }

                modalContainer.innerHTML = `<div class="modal fade" id="genreViewModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-xl">
                    ${html}
                </div>
            </div>`;

                const modal = new bootstrap.Modal(document.getElementById('genreViewModal'));
                modal.show();
            })
            .catch(error => {
                console.error('Error loading genre view modal:', error);
            });
    }


    return {
        init: function (config) {
            settings = config;
            initializeEventHandlers();
            const initialParams = new URLSearchParams(window.location.search);
            if (initialParams.toString()) {
                loadFilteredResults();
            }
        }
    };
})();

$(function () {
    GenreMasterScreen.init({
        genreMasterScreenUrl: '/GenreMaster/GenreMasterScreen',
        genreAddModalUrl: '/GenreMaster/GenreAddModal',
        genreEditModalUrl: '/GenreMaster/GenreEditModal',
        createGenreUrl: '/GenreMaster/Create',
        editGenreUrl: '/GenreMaster/Edit',
        deleteGenreUrl: '/GenreMaster/Delete'
    });
});
