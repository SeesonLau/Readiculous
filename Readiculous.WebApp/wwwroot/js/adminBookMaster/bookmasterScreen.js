// Cache temporary fix for genreFilter response time

const BookMasterScreen = (function () {
    let allBooks = []; // Cache for all books
    let currentFilters = {
        searchString: '',
        genreFilters: {},
        sortOrder: ''
    };

    function init(config) {
        // Cache all books on initial load
        cacheAllBooks();

        const bookSettings = {
            masterScreenUrl: config.bookMasterScreenUrl || window.location.pathname,
            listContainer: '#bookListContainer',
            addModalUrl: config.bookAddModalUrl,
            editModalUrl: config.bookEditModalUrl,
            viewModalUrl: config.bookViewModalUrl,
            deleteUrl: config.deleteBookUrl,
            filterForm: '#bookFilterForm'
        };

        CommonMaster.init(bookSettings);
        initGenreDropdown();
        initEventHandlers(bookSettings);

        // Initialize from URL params if present
        initFromUrlParams();
    }

    function cacheAllBooks() {
        allBooks = [];
        $('.book-table tbody tr').each(function () {
            const $row = $(this);
            const genres = $row.find('.genre-badge').map(function () {
                return $(this).text().trim().toLowerCase();
            }).get();

            allBooks.push({
                element: $row,
                title: $row.find('.title-cell').text().trim().toLowerCase(),
                genres: genres,
                author: $row.find('.author-cell').text().trim().toLowerCase(),
                date: $row.find('.date-cell').data('sort-value') || ''
            });
        });
    }

    function initFromUrlParams() {
        const urlParams = new URLSearchParams(window.location.search);

        // Search string
        if (urlParams.has('searchString')) {
            currentFilters.searchString = urlParams.get('searchString').toLowerCase();
            $('input[name="searchString"]').val(urlParams.get('searchString'));
        }

        // Sort order
        if (urlParams.has('sortOrder')) {
            currentFilters.sortOrder = urlParams.get('sortOrder');
            $('select[name="sortOrder"]').val(urlParams.get('sortOrder'));
        }

        // Genre filters
        if (urlParams.has('genreFilter')) {
            const genreFilterParam = urlParams.get('genreFilter');
            genreFilterParam.split(',').forEach(filter => {
                const action = filter[0] === '+' ? 'include' : 'exclude';
                const genre = decodeURIComponent(filter.substring(1)).toLowerCase();
                currentFilters.genreFilters[genre] = action;

                // Update UI for genre checkboxes
                $(`.genre-check-item`).each(function () {
                    const $item = $(this);
                    const itemGenre = $item.find('.genre-check-label').text().trim().toLowerCase();
                    if (itemGenre === genre) {
                        $item.attr('data-state', action === 'include' ? 'included' : 'excluded');
                        $item.find('.genre-checkbox').html(action === 'include' ? '✓' : '✕');
                    }
                });
            });
            updateSelectedCount();
        }

        // Apply initial filters
        applyFilters();
    }

    function initGenreDropdown() {
        const $dropdownToggle = $('.genre-dropdown-toggle');
        const $dropdownMenu = $('.genre-dropdown-menu');
        const $selectedCount = $('.selected-genres-count');
        const $selectedText = $('.selected-genres-text');

        // Toggle dropdown
        $dropdownToggle.on('click', function (e) {
            e.stopPropagation();
            $(this).toggleClass('open');
            $dropdownMenu.toggleClass('open');
        });

        // Close when clicking outside
        $(document).on('click', function (e) {
            if (!$(e.target).closest('.genre-filter-container').length) {
                $dropdownToggle.removeClass('open');
                $dropdownMenu.removeClass('open');
            }
        });

        // Initialize genre items
        $('.genre-check-item').on('click', function () {
            const $this = $(this);
            const currentState = $this.attr('data-state') || 'unchecked';
            const genreName = $this.find('.genre-check-label').text().trim().toLowerCase();

            // Cycle through states
            if (currentState === 'unchecked') {
                $this.attr('data-state', 'included');
                currentFilters.genreFilters[genreName] = 'include';
                $this.find('.genre-checkbox').html('✓');
            } else if (currentState === 'included') {
                $this.attr('data-state', 'excluded');
                currentFilters.genreFilters[genreName] = 'exclude';
                $this.find('.genre-checkbox').html('✕');
            } else {
                $this.attr('data-state', 'unchecked');
                delete currentFilters.genreFilters[genreName];
                $this.find('.genre-checkbox').html('');
            }

            updateSelectedCount();
            applyFilters();
            updateUrlParams();
        });

        function updateSelectedCount() {
            const count = Object.keys(currentFilters.genreFilters).length;
            $selectedCount.text(count).toggle(count > 0);
            $selectedText.text(count === 0 ? 'Select genres' :
                count === 1 ? '1 genre selected' : `${count} genres selected`);
        }
    }

    function initEventHandlers(bookSettings) {
        // Add Book
        $(document).on('click', '.btn-add-book', function () {
            $('#addBookModal').modal('show');
            CommonMaster.loadModalContent('#addBookModalBody', bookSettings.addModalUrl, '#addBookForm');
        });

        // Edit Book
        $(document).on('click', '.btn-edit-book', function () {
            const bookId = $(this).data('bookid');
            $('#editBookModal').modal('show');
            CommonMaster.loadModalContent('#editBookModalBody', `${bookSettings.editModalUrl}?id=${bookId}`, '#editBookForm');
        });

        // View Book
        $(document).on('click', '.btn-view-book', function () {
            const bookId = $(this).data('bookid');
            $('#viewBookModal').modal('show');
            CommonMaster.loadModalContent('#viewBookModalBody', `${bookSettings.viewModalUrl}?id=${bookId}`);
        });

        // Delete Book
        $(document).on('click', '.btn-delete-book', function () {
            const bookId = $(this).data('bookid');
            const bookName = $(this).data('bookname');
            $('#itemToDeleteId').val(bookId);
            $('#itemToDeleteName').text(bookName);
            $('#deleteModal').modal('show');
        });

        // Search Input
        $(document).on('input', 'input[name="searchString"]', CommonMaster.debounce(function () {
            CommonMaster.loadFilteredResults();
        }, 300));


        // Sort Order
        $(document).on('change', 'select[name="sortOrder"]', CommonMaster.debounce(function () {
            CommonMaster.loadFilteredResults();
        }, 300));
    }

    function applyFilters() {
        const filteredBooks = allBooks.filter(book => {
            // Search filter
            if (currentFilters.searchString &&
                !book.title.includes(currentFilters.searchString)) {
                return false;
            }

            // Genre filters
            const genreFilters = currentFilters.genreFilters;
            if (Object.keys(genreFilters).length > 0) {
                for (const [genre, action] of Object.entries(genreFilters)) {
                    const hasGenre = book.genres.includes(genre.toLowerCase());
                    if ((action === 'include' && !hasGenre) ||
                        (action === 'exclude' && hasGenre)) {
                        return false;
                    }
                }
            }

            return true;
        });

        // Sort if needed
        if (currentFilters.sortOrder) {
            filteredBooks.sort(getSortComparator(currentFilters.sortOrder));
        }

        // Update UI
        $('.book-table tbody tr').hide();
        filteredBooks.forEach(book => book.element.show());
    }

    function getSortComparator(sortOrder) {
        switch (sortOrder) {
            case 'title_asc':
                return (a, b) => a.title.localeCompare(b.title);
            case 'title_desc':
                return (a, b) => b.title.localeCompare(a.title);
            case 'author_asc':
                return (a, b) => a.author.localeCompare(b.author);
            case 'author_desc':
                return (a, b) => b.author.localeCompare(a.author);
            case 'date_asc':
                return (a, b) => new Date(a.date) - new Date(b.date);
            case 'date_desc':
                return (a, b) => new Date(b.date) - new Date(a.date);
            default:
                return () => 0;
        }
    }

    function updateUrlParams() {
        const urlParams = new URLSearchParams();

        if (currentFilters.searchString) {
            urlParams.set('searchString', currentFilters.searchString);
        }

        if (currentFilters.sortOrder) {
            urlParams.set('sortOrder', currentFilters.sortOrder);
        }

        // Convert genre filters to URL format
        const genreParams = [];
        for (const [genre, action] of Object.entries(currentFilters.genreFilters)) {
            genreParams.push(`${action === 'include' ? '+' : '-'}${encodeURIComponent(genre)}`);
        }

        if (genreParams.length > 0) {
            urlParams.set('genreFilter', genreParams.join(','));
        }

        const newUrl = `${window.location.pathname}?${urlParams.toString()}`;
        window.history.replaceState({ path: newUrl }, '', newUrl);
    }

    return { init };
})();

$(function () {
    const defaultSettings = {
        bookMasterScreenUrl: window.location.pathname,
        bookAddModalUrl: '/BookMaster/BookAddModal',
        bookEditModalUrl: '/BookMaster/BookEditModal',
        bookViewModalUrl: '/BookMaster/BookViewModal',
        deleteBookUrl: '/BookMaster/Delete'
    };

    const settings = typeof bookMasterSettings !== 'undefined' ?
        { ...defaultSettings, ...bookMasterSettings } : defaultSettings;

    BookMasterScreen.init(settings);
});

