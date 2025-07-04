const GenreMasterScreen = (function () {
    let settings = {};
    const loadingSpinner = `
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-2">Loading genres...</p>
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
        // Pagination handlers
        $(document).on('click', '[data-page-size]', function (e) {
            e.preventDefault();
            const pageSize = $(this).data('page-size');
            updateUrlWithPageSize(pageSize);
        });

        $(document).on('click', '.pagination .page-link:not(.jump-to-page)', function (e) {
            e.preventDefault();
            const page = $(this).data('page');
            if (page && !$(this).closest('.page-item').hasClass('disabled')) {
                updateUrlWithPage(page);
            }
        });

        // Jump to page handler
        $('#confirmJumpToPage').on('click', function () {
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
        $('input[name="searchString"]').on('input', debounce(loadFilteredResults, 300));
        $('select[name="searchType"]').on('change', loadFilteredResults);

        // Modal handlers
        $(document).on('click', '.btn-delete-genre', function () {
            const genreId = $(this).data('genre-id');
            const genreName = $(this).data('genre-name');
            $('#userToDeleteId').val(genreId);
            $('#userToDeleteName').text(genreName);
            $('#deleteUserModal').modal('show');
        });

        $('#cancelDeleteGenreBtn').on('click', function () {
            $('#deleteUserModal').modal('hide');
        });

        $('#openAddGenreModalBtn').on('click', function () {
            $('#addGenreOverlay').fadeIn(150);
        });

        $('#cancelAddGenreBtn').on('click', function () {
            $('#addGenreOverlay').fadeOut(150);
        });

        $('#addGenreForm').on('submit', handleAddGenreSubmit);

        $(document).on('click', '.edit-genre-btn', function () {
            const genreId = $(this).data('genre-id');
            const genreName = $(this).data('genre-name');
            const genreDescription = $(this).data('genre-description');
            $('#editGenreId').val(genreId);
            $('#editGenreName').val(genreName);
            $('#editGenreDescription').val(genreDescription);
            $('#editGenreValidationSummary').text('');
            $('#editGenreNameError').text('');
            $('#editGenreDescriptionError').text('');
            $('#editGenreOverlay').fadeIn(150);
        });

        $('#cancelEditGenreBtn').on('click', function () {
            $('#editGenreOverlay').fadeOut(150);
        });

        $('#editGenreForm').on('submit', handleEditGenreSubmit);
        $('#confirmDeleteBtn').on('click', handleDeleteConfirmation);
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
        url.searchParams.set('page', '1'); // Reset to first page when changing page size
        window.history.replaceState({ path: url.toString() }, '', url.toString());
        loadFilteredResults();
    }

    async function loadFilteredResults() {
        const form = $('#genreFilterForm');
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

    function handleAddGenreSubmit(e) {
        e.preventDefault();
        const form = $(this);
        const submitBtn = form.find('button[type="submit"]');
        const originalBtnText = submitBtn.html();

        const name = $('#addGenreName').val().trim();
        const description = $('#addGenreDescription').val().trim();
        let isValid = true;

        $('#addGenreValidationSummary').text('');
        $('#addGenreNameError').text('');
        $('#addGenreDescriptionError').text('');

        if (!name) {
            $('#addGenreNameError').text('Genre Name is required.');
            isValid = false;
        }
        if (!description) {
            $('#addGenreDescriptionError').text('Genre Description is required.');
            isValid = false;
        }
        if (!isValid) return;

        submitBtn.prop('disabled', true).html(`
            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            Processing...
        `);

        $.ajax({
            url: settings.addGenreUrl,
            type: 'POST',
            data: { Name: name, Description: description },
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            success: function (result) {
                if (result.success) {
                    // Preserve pageSize on add
                    const urlParams = new URLSearchParams(window.location.search);
                    const pageSize = urlParams.get('pageSize') || '10';
                    const newUrl = new URL(window.location.href);
                    newUrl.searchParams.set('pageSize', pageSize);
                    window.location.href = newUrl.toString();
                } else {
                    $('#addGenreValidationSummary').text(result.message || 'Failed to add genre.');
                }
            },
            error: function (xhr) {
                const response = xhr.responseJSON;
                if (response && response.errors) {
                    if (response.errors.Name) $('#addGenreNameError').text(response.errors.Name);
                    if (response.errors.Description) $('#addGenreDescriptionError').text(response.errors.Description);
                } else {
                    $('#addGenreValidationSummary').text('An error occurred while adding the genre.');
                }
            },
            complete: function () {
                submitBtn.prop('disabled', false).html(originalBtnText);
            }
        });
    }

    function handleEditGenreSubmit(e) {
        e.preventDefault();
        const form = $(this);
        const submitBtn = form.find('button[type="submit"]');
        const originalBtnText = submitBtn.html();

        const genreId = $('#editGenreId').val();
        const name = $('#editGenreName').val().trim();
        const description = $('#editGenreDescription').val().trim();
        let isValid = true;

        $('#editGenreValidationSummary').text('');
        $('#editGenreNameError').text('');
        $('#editGenreDescriptionError').text('');

        if (!name) {
            $('#editGenreNameError').text('Genre Name is required.');
            isValid = false;
        }
        if (!description) {
            $('#editGenreDescriptionError').text('Genre Description is required.');
            isValid = false;
        }
        if (!isValid) return;

        submitBtn.prop('disabled', true).html(`
            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            Processing...
        `);

        $.ajax({
            url: settings.editGenreUrl,
            type: 'POST',
            data: { GenreId: genreId, Name: name, Description: description },
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            success: function (result) {
                if (result.success) {
                    // Preserve pageSize on edit
                    const urlParams = new URLSearchParams(window.location.search);
                    const pageSize = urlParams.get('pageSize') || '10';
                    const newUrl = new URL(window.location.href);
                    newUrl.searchParams.set('pageSize', pageSize);
                    window.location.href = newUrl.toString();
                } else {
                    $('#editGenreValidationSummary').text(result.message || 'Failed to update genre.');
                }
            },
            error: function (xhr) {
                const response = xhr.responseJSON;
                if (response && response.errors) {
                    if (response.errors.Name) $('#editGenreNameError').text(response.errors.Name);
                    if (response.errors.Description) $('#editGenreDescriptionError').text(response.errors.Description);
                } else {
                    $('#editGenreValidationSummary').text('An error occurred while updating the genre.');
                }
            },
            complete: function () {
                submitBtn.prop('disabled', false).html(originalBtnText);
            }
        });
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
                    // Preserve pageSize on delete
                    const urlParams = new URLSearchParams(window.location.search);
                    const pageSize = urlParams.get('pageSize') || '10';
                    const newUrl = new URL(window.location.href);
                    newUrl.searchParams.set('pageSize', pageSize);
                    window.location.href = newUrl.toString();
                } else {
                    showErrorAlert('Error deleting genre: ' + (result.message || 'Unknown error'));
                }
            },
            error: function (xhr) {
                showErrorAlert('An error occurred while deleting the genre: ' + (xhr.responseJSON?.message || xhr.statusText));
            },
            complete: function () {
                submitBtn.prop('disabled', false).html(originalBtnText);
                $('#deleteUserModal').modal('hide');
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

        $('.container-fluid').prepend(alertHtml);
        setTimeout(() => $('.alert').alert('close'), 5000);
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
        genreMasterScreenUrl: window.location.pathname,
        addGenreUrl: '@Url.Action("Create", "Genre")',
        editGenreUrl: '@Url.Action("Edit", "Genre")',
        deleteGenreUrl: '@Url.Action("Delete", "Genre")'
    });
});
