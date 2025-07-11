const BookMasterScreen = (function () {
    let settings = {};
    const loadingSpinner = `
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-2">Loading books...</p>
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
        // Pagination
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

        // Modify the loadFilteredResults function to include pagination params
        async function loadFilteredResults() {
            const form = $('#bookFilterForm');
            const urlParams = new URLSearchParams(window.location.search);
            const page = urlParams.get('page') || '1';
            const pageSize = urlParams.get('pageSize') || '10';

            const formData = form.serialize() + `&page=${page}&pageSize=${pageSize}`;

            $('#bookListContainer').html(loadingSpinner);

            try {
                const response = await $.ajax({
                    url: settings.bookMasterScreenUrl,
                    type: 'GET',
                    data: formData,
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });

                updateUrlWithFilters(formData);
                const content = $(response).find('#bookListContainer').html() || response;
                $('#bookListContainer').html(content);

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
                showErrorAlert('Failed to load books. Please try again.');
            }
        }

        // FILTERS

        const filterForm = $('#bookFilterForm');

        $('input[name="searchString"]').on('input', debounce(loadFilteredResults, 300));
        $('select[name="searchType"], select[name="genreFilter"], select[name="sortOrder"]').on('change', loadFilteredResults);
        $(document).on('change', 'input[name="selectedGenreIds"]', debounce(loadFilteredResults, 300));

        $(document).on('click', '.btn-add-book', function () {
            $('#addBookModalBody').html(loadingSpinner);
            $('#addBookModal').modal('show');
            $('#addBookModalBody').load(settings.bookAddModalUrl, function () {
                $.validator.unobtrusive.parse('#addBookForm');
            });
        });

        $(document).on('click', '.btn-edit-book', function () {
            const bookId = $(this).data('bookid');
            $('#editBookModalBody').html(loadingSpinner);
            $('#editBookModal').modal('show');
            $('#editBookModalBody').load(`${settings.bookEditModalUrl}?id=${bookId}`, function () {
                $.validator.unobtrusive.parse('#editBookForm');
            });
        });

        $(document).on('click', '.btn-view-book', function () {
            const bookId = $(this).data('bookid');
            $('#viewBookModalBody').html(loadingSpinner);
            $('#viewBookModal').modal('show');
            $('#viewBookModalBody').load(`${settings.bookViewModalUrl}?id=${bookId}`);
        });

        $(document).on('click', '.btn-delete-book', function () {
            const bookId = $(this).data('bookid');
            const bookName = $(this).data('bookname');
            $('#userToDeleteId').val(bookId);
            $('#userToDeleteName').text(bookName);
            $('#deleteUserModal').modal('show');
        });

        $(document).on('submit', '#addBookForm, #editBookForm', handleFormSubmit);
        $('#confirmDeleteBtn').click(handleDeleteConfirmation);
    }

    async function loadFilteredResults() {
        const form = $('#bookFilterForm');
        const formData = form.serialize();

        $('#bookListContainer').html(loadingSpinner);

        try {
            const response = await $.ajax({
                url: settings.bookMasterScreenUrl,
                type: 'GET',
                data: formData,
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            updateUrlWithFilters(formData);
            const content = $(response).find('#bookListContainer').html() || response;
            $('#bookListContainer').html(content);
        } catch (error) {
            showErrorAlert('Failed to load books. Please try again.');
        }
    }

    function updateUrlWithFilters(params) {
        const newUrl = `${window.location.pathname}?${params}`;
        window.history.replaceState({ path: newUrl }, '', newUrl);
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

        const formData = new FormData(form[0]);

        $.ajax({
            url: form.attr('action'),
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    // Get current pageSize from URL or use default
                    const urlParams = new URLSearchParams(window.location.search);
                    const pageSize = urlParams.get('pageSize') || '10';

                    // Reload with the preserved pageSize
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
        const bookId = $('#userToDeleteId').val();

        btn.prop('disabled', true).html(`
        <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
        Deleting...
    `);

        $.post(settings.deleteBookUrl, { id: bookId })
            .done(function () {
                // Preserve pageSize on delete
                const urlParams = new URLSearchParams(window.location.search);
                const pageSize = urlParams.get('pageSize') || '10';
                const newUrl = new URL(window.location.href);
                newUrl.searchParams.set('pageSize', pageSize);
                window.location.href = newUrl.toString();
            })
            .fail(function () {
                showErrorAlert('Failed to delete book');
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

        $('#bookListContainer').prepend(alertHtml);
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
    if (typeof bookMasterSettings !== 'undefined') {
        if (!bookMasterSettings.bookMasterScreenUrl) {
            bookMasterSettings.bookMasterScreenUrl = window.location.pathname;
        }
        BookMasterScreen.init(bookMasterSettings);
    } else {
        BookMasterScreen.init({
            bookMasterScreenUrl: window.location.pathname,
            bookAddModalUrl: '/BookMaster/BookAddModal',
            bookEditModalUrl: '/BookMaster/BookEditModal',
            bookViewModalUrl: '/BookMaster/BookViewModal',
            deleteBookUrl: '/BookMaster/Delete'
        });
    }
});
