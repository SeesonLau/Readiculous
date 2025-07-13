//CommonMaster - Centralized functionality for all master screens
//Handles filtering, sorting, pagination, modals, and common operations

const CommonMaster = (function () {
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

    function initializeCommonEventHandlers() {
        // Page size handling
        $(document).on('click', '[data-page-size]', function (e) {
            e.preventDefault();
            updateUrlWithPageSize($(this).data('page-size'));
        });

        // Pagination handling
        $(document).on('click', '.pagination .page-link:not(.jump-to-page)', function (e) {
            e.preventDefault();
            const page = $(this).data('page');
            if (page && !$(this).closest('.page-item').hasClass('disabled')) {
                updateUrlWithPage(page);
            }
        });

        // Jump to page handling
        $(document).on('click', '#confirmJumpToPage', handleJumpToPage);

        // Delete confirmation
        $(document).on('click', '#confirmDeleteBtn', handleDeleteConfirmation);
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

    function handleJumpToPage() {
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
    }

    async function loadFilteredResults() {
        const form = $(settings.filterForm || '#filterForm');
        const urlParams = new URLSearchParams(window.location.search);
        const page = urlParams.get('page') || '1';
        const pageSize = urlParams.get('pageSize') || '10';

        const formData = form.serialize() + `&page=${page}&pageSize=${pageSize}`;

        $(settings.listContainer).html(loadingSpinner);

        try {
            const response = await $.ajax({
                url: settings.masterScreenUrl,
                type: 'GET',
                data: formData,
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            updateUrlWithFilters(formData);
            const content = $(response).find(settings.listContainer).html() || response;
            $(settings.listContainer).html(content);

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
            showErrorAlert('Failed to load data. Please try again.');
        }
    }

    function updateUrlWithFilters(params) {
        const newUrl = `${window.location.pathname}?${params}`;
        window.history.replaceState({ path: newUrl }, '', newUrl);
    }

    function loadModalContent(modalBodyId, url, formId = null) {
        $(modalBodyId).html(loadingSpinner);
        $(modalBodyId).load(url, function () {
            if (formId) {
                $.validator.unobtrusive.parse(formId);
            }
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
        const itemId = $('#itemToDeleteId').val();

        btn.prop('disabled', true).html(`
            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            Deleting...
        `);

        $.post(settings.deleteUrl, { id: itemId })
            .done(function () {
                window.location.reload();
            })
            .fail(function () {
                showErrorAlert('Failed to delete item');
            })
            .always(function () {
                btn.prop('disabled', false).html(originalBtnText);
                $('#deleteModal').modal('hide');
            });
    }

    function showErrorAlert(message) {
        const alertHtml = `
            <div class="alert alert-danger alert-dismissible fade show" role="alert">
                <i class="fas fa-exclamation-triangle me-2"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>`;

        $(settings.listContainer).prepend(alertHtml);
        setTimeout(() => $('.alert').alert('close'), 5000);
    }

    return {
        init: function (config) {
            settings = config;
            initializeCommonEventHandlers();

            // Initialize filter inputs from URL
            const urlParams = new URLSearchParams(window.location.search);
            ['searchString', 'sortOrder', 'roleType', 'genreFilter'].forEach(param => {
                if (urlParams.has(param) && $(`#${param}`).length) {
                    $(`#${param}`).val(urlParams.get(param));
                }
            });

            // Set up search input debounce
            if ($('#searchString').length) {
                $('#searchString').on('input', debounce(loadFilteredResults, 300));
            }

            // Set up sort order change handler
            if ($('#sortOrder').length) {
                $('#sortOrder').on('change', loadFilteredResults); 
            }
            // Set up roleType change handler
            if ($('#roleType').length) {
                $('#roleType').on('change', loadFilteredResults); 
            }
            // Set up roleType change handler
            if ($('#genreFilter').length) {
                $('#genreFilter').on('change', loadFilteredResults);
            }

            // Load initial results if URL has parameters
            if (urlParams.toString()) {
                loadFilteredResults();
            }
        },
        loadModalContent: loadModalContent,
        showErrorAlert: showErrorAlert,
        debounce: debounce,
        loadFilteredResults: loadFilteredResults
    };
})();
