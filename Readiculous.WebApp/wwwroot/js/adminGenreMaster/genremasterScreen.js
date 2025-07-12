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

        // Filter handlers
        $(document).on('input', '#searchString', debounce(loadFilteredResults, 300));
        $(document).on('change', '#sortType', loadFilteredResults);

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

        // Form submission handlers
        $(document).on('submit', '#addGenreForm, #editGenreForm', handleFormSubmit);
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

            updateUrlWithFilters(formData);
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

    function updateUrlWithFilters(params) {
        const newUrl = `${window.location.pathname}?${params}`;
        window.history.replaceState({ path: newUrl }, '', newUrl);
    }

    function loadAddGenreModal() {
        $('#addGenreModalBody').html(loadingSpinner);
        $('#addGenreModal').modal('show');
        $('#addGenreModalBody').load(settings.genreAddModalUrl, function () {
            $.validator.unobtrusive.parse('#addGenreForm');
        });
    }

    function loadEditGenreModal(genreId) {
        $('#editGenreModalBody').html(loadingSpinner);
        $('#editGenreModal').modal('show');
        $('#editGenreModalBody').load(`${settings.genreEditModalUrl}?id=${genreId}`, function () {
            $.validator.unobtrusive.parse('#editGenreForm');
        });
    }

    function handleFormSubmit(e) {
        e.preventDefault();
        const form = $(this);
        const submitBtn = form.find('button[type="submit"]');
        const originalBtnText = submitBtn.html();

        if (!form.valid()) {
            showErrorAlert('Please fill in all required fields correctly.');
            return;
        }

        submitBtn.prop('disabled', true).html(`
            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            Processing...
        `);

        $.ajax({
            url: form.attr('action'),
            type: 'POST',
            data: form.serialize(),
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            success: function (response) {
                if (response.success) {
                    // Preserve pageSize on form submission
                    const urlParams = new URLSearchParams(window.location.search);
                    const pageSize = urlParams.get('pageSize') || '10';
                    const newUrl = new URL(window.location.href);
                    newUrl.searchParams.set('pageSize', pageSize);
                    window.location.href = newUrl.toString();
                } else {
                    const modalBody = form.closest('.modal-body').attr('id');
                    $(`#${modalBody}`).html(response);
                    $.validator.unobtrusive.parse(form);
                }
            },
            error: function () {
                showErrorAlert('Request failed. Please try again.');
            },
            complete: function () {
                submitBtn.prop('disabled', false).html(originalBtnText);
            }
        });
    }

    function handleDeleteConfirmation() {
        const btn = $(this);
        const originalBtnText = btn.html();
        const genreId = $('#userToDeleteId').val();

        btn.prop('disabled', true).html(`
            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            Deleting...
        `);

        $.post(settings.deleteGenreUrl, { id: genreId })
            .done(function () {
                // Preserve pageSize on delete
                const urlParams = new URLSearchParams(window.location.search);
                const pageSize = urlParams.get('pageSize') || '10';
                const newUrl = new URL(window.location.href);
                newUrl.searchParams.set('pageSize', pageSize);
                window.location.href = newUrl.toString();
            })
            .fail(function () {
                showErrorAlert('Failed to delete genre');
            })
            .always(function () {
                btn.prop('disabled', false).html(originalBtnText);
                $('#deleteUserModal').modal('hide');
            });
    }

    function showErrorAlert(message) {
        const alertHtml = `
            <div class="alert alert-danger alert-dismissible fade show" role="alert">
                <i class="fas fa-exclamation-triangle me-2"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>`;

        $('#genreListContainer').prepend(alertHtml);
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
    if (typeof genreMasterSettings !== 'undefined') {
        if (!genreMasterSettings.genreMasterScreenUrl) {
            genreMasterSettings.genreMasterScreenUrl = window.location.pathname;
        }
        GenreMasterScreen.init(genreMasterSettings);
    } else {
        GenreMasterScreen.init({
            genreMasterScreenUrl: window.location.pathname,
            genreAddModalUrl: '/GenreMaster/GenreAddModal',
            genreEditModalUrl: '/GenreMaster/GenreEditModal',
            genreViewModalUrl: '/GenreMaster/GenreViewModal',
            deleteGenreUrl: '/GenreMaster/Delete'
        });
    }
});