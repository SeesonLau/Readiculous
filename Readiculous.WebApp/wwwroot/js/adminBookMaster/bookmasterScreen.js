const BookMasterScreen = (function () {
    function init(config) {
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

        $(document).on('click', '.btn-add-book', function () {
            $('#addBookModal').modal('show');
            CommonMaster.loadModalContent('#addBookModalBody', bookSettings.addModalUrl, '#addBookForm');
        });

        $(document).on('click', '.btn-edit-book', function () {
            const bookId = $(this).data('bookid');
            $('#editBookModal').modal('show');
            CommonMaster.loadModalContent('#editBookModalBody', `${bookSettings.editModalUrl}?id=${bookId}`, '#editBookForm');
        });

        $(document).on('click', '.btn-view-book', function () {
            const bookId = $(this).data('bookid');
            $('#viewBookModal').modal('show');
            CommonMaster.loadModalContent('#viewBookModalBody', `${bookSettings.viewModalUrl}?id=${bookId}`);
        });

        $(document).on('click', '.btn-delete-book', function () {
            const bookId = $(this).data('bookid');
            const bookName = $(this).data('bookname');
            $('#itemToDeleteId').val(bookId);
            $('#itemToDeleteName').text(bookName);
            $('#deleteModal').modal('show');
        });

        $(document).on('input', 'input[name="searchString"]', CommonMaster.debounce(function () {
            CommonMaster.loadFilteredResults();
        }, 300));

        $(document).on('change', 'select[name="genreFilter"]', CommonMaster.debounce(function () {
            CommonMaster.loadFilteredResults();
        }, 300));

        $(document).on('change', 'select[name="sortOrder"]', CommonMaster.debounce(function () {
            CommonMaster.loadFilteredResults();
        }, 300));
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
