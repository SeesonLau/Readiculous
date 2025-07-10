// genreViewPage.js
const GenreViewPage = (function () {
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
        // Book search handler
        $(document).on('input', '#bookSearchForm input[name="bookSearch"]', debounce(function () {
            updateUrlAndLoad({ page: 1, bookSearch: $(this).val() });
        }, 300));

        $(document).on('submit', '#bookSearchForm', function (e) {
            e.preventDefault();
            updateUrlAndLoad({ page: 1, bookSearch: $(this).find('input[name="bookSearch"]').val() });
        });

        // Show jump to page modal
        $(document).on('click', '.genre-main-container .pagination-container .jump-to-page', function (e) {
            e.preventDefault();
            const totalPages = $(this).closest('.pagination').data('total-pages');
            $('#jumpToPageModal #pageNumberInput').attr({
                'min': 1,
                'max': totalPages,
                'placeholder': `Enter page (1-${totalPages})`
            }).val('');
            $('#pageNumberError').text('');
            $('#jumpToPageModal').modal('show');
        });

        // Jump to page handling
        $(document).on('click', '#confirmJumpToPage', function (e) {
            e.preventDefault();

            const pageNumberInput = $('#pageNumberInput');
            const pageNumberError = $('#pageNumberError');
            const pageNumber = parseInt(pageNumberInput.val());
            const totalPages = parseInt(pageNumberInput.attr('max'));

            // Clear validation
            pageNumberInput.removeClass('is-invalid');
            pageNumberError.text('');

            // Validate input
            if (isNaN(pageNumber)) {
                pageNumberInput.addClass('is-invalid');
                pageNumberError.text('Please enter a valid page number.');
                return;
            }

            if (pageNumber < 1 || pageNumber > totalPages) {
                pageNumberInput.addClass('is-invalid');
                pageNumberError.text(`Please enter a number between 1 and ${totalPages}.`);
                return;
            }

            $('#jumpToPageModal').modal('hide');
            updateUrlAndLoad({ page: pageNumber });
        });

        // Prevent non-numeric input
        $(document).on('input', '#pageNumberInput', function () {
            this.value = this.value.replace(/[^0-9]/g, '');
        });

        // Handle browser back/forward navigation
        window.addEventListener('popstate', function () {
            loadFromUrl();
        });
    }

    function updateUrlAndLoad(params = {}) {
        const currentUrl = new URL(window.location.href);
        const currentParams = new URLSearchParams(currentUrl.search);

        // Always include the genre ID
        if (!currentParams.has('id')) {
            currentParams.set('id', settings.genreId);
        }

        // Update parameters
        if (params.page !== undefined) {
            // Special case: page 1 - we'll remove the page parameter
            if (params.page == 1) {
                currentParams.delete('page');
            } else {
                currentParams.set('page', params.page);
            }
        }

        if (params.bookSearch !== undefined) {
            if (params.bookSearch) {
                currentParams.set('bookSearch', params.bookSearch);
            } else {
                currentParams.delete('bookSearch');
            }
        }

        // Always include pageSize for consistency
        currentParams.set('pageSize', 10);

        // Update URL
        currentUrl.search = currentParams.toString();

        // Only push state if URL actually changed
        if (currentUrl.href !== window.location.href) {
            window.history.pushState({}, '', currentUrl);
        }

        // Load content
        loadFilteredBooks(params);
    }

    function loadFromUrl() {
        const urlParams = new URLSearchParams(window.location.search);
        loadFilteredBooks({
            page: urlParams.get('page'),
            bookSearch: urlParams.get('bookSearch')
        });
    }

    async function loadFilteredBooks(params = {}) {
        const urlParams = new URLSearchParams(window.location.search);
        const dataToSend = {
            id: settings.genreId,
            page: params.page || urlParams.get('page') || '1',
            bookSearch: params.bookSearch || urlParams.get('bookSearch') || settings.initialBookSearch || '',
            pageSize: 10
        };

        $('#genreBookListContainer').html(loadingSpinner);

        try {
            const response = await $.ajax({
                url: settings.genreViewPageUrl,
                type: 'GET',
                data: dataToSend,
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            const responseHtml = $(response);
            const bookListHtml = responseHtml.find('#genreBookListContainer').html();
            const paginationHtml = responseHtml.find('.pagination-container').html();

            $('#genreBookListContainer').html(bookListHtml);
            $('.genre-main-container .pagination-container').html(paginationHtml);
        } catch (error) {
            console.error('Error loading books:', error);
            showErrorAlert('Failed to load books. Please try again.');
        }
    }

    function showErrorAlert(message) {
        const alertHtml = `
            <div class="alert alert-danger alert-dismissible fade show" role="alert">
                <i class="fas fa-exclamation-triangle me-2"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>`;
        $('.genre-main-container').prepend(alertHtml);
        setTimeout(() => $('.genre-main-container .alert').alert('close'), 5000);
    }

    return {
        init: function (config) {
            settings = config;
            initializeEventHandlers();
            loadFromUrl();
        }
    };
})();

$(document).ready(function () {
    if (typeof genreViewPageSettings !== 'undefined') {
        GenreViewPage.init(genreViewPageSettings);
    }
});
