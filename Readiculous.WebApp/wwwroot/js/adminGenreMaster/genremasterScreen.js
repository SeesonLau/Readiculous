const GenreMasterScreen = (function () {
    function init(config) {
        const genreSettings = {
            masterScreenUrl: config.genreMasterScreenUrl || window.location.pathname,
            listContainer: '#genreListContainer',
            addModalUrl: config.genreAddModalUrl,
            editModalUrl: config.genreEditModalUrl,
            viewModalUrl: config.genreViewModalUrl,
            deleteUrl: config.deleteGenreUrl,
            filterForm: '#genreFilterForm'
        };

        CommonMaster.init(genreSettings);

        // Genre-specific event handlers
        $(document).on('click', '#openAddGenreModalBtn', function () {
            $('#addGenreModal').modal('show');
            CommonMaster.loadModalContent('#addGenreModalBody', genreSettings.addModalUrl, '#addGenreForm');
        });

        $(document).on('click', '.edit-genre-btn', function (e) {
            e.preventDefault();
            const genreId = $(this).data('genre-id');
            $('#editGenreModal').modal('show');
            CommonMaster.loadModalContent('#editGenreModalBody', `${genreSettings.editModalUrl}?id=${genreId}`, '#editGenreForm');
        });

        $(document).on('click', '.btn-delete-genre', function () {
            const genreId = $(this).data('genre-id');
            const genreName = $(this).data('genre-name');
            $('#itemToDeleteId').val(genreId);
            $('#itemToDeleteName').text(genreName);
            $('#deleteModal').modal('show');
        });
    }

    return { init };
})();

$(function () {
    const defaultSettings = {
        genreMasterScreenUrl: window.location.pathname,
        genreAddModalUrl: '/GenreMaster/GenreAddModal',
        genreEditModalUrl: '/GenreMaster/GenreEditModal',
        genreViewModalUrl: '/GenreMaster/GenreViewModal',
        deleteGenreUrl: '/GenreMaster/Delete'
    };

    const settings = typeof genreMasterSettings !== 'undefined' ?
        { ...defaultSettings, ...genreMasterSettings } : defaultSettings;

    GenreMasterScreen.init(settings);
});
