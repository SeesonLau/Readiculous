const UserMasterScreen = (function () {
    function init(config) {
        const userSettings = {
            masterScreenUrl: config.userMasterScreenUrl || window.location.pathname,
            listContainer: '#userListContainer',
            addModalUrl: config.userAddModalUrl,
            editModalUrl: config.userEditModalUrl,
            viewModalUrl: config.userViewModalUrl,
            deleteUrl: config.deleteUserUrl,
            filterForm: '#userFilterForm'
        };

        CommonMaster.init(userSettings);

        // User-specific event handlers
        $(document).on('click', '[data-bs-target="#addUserModal"]', function () {
            $('#addUserModal').modal('show');
            CommonMaster.loadModalContent('#addUserModalBody', userSettings.addModalUrl, '#addUserForm');
        });

        $(document).on('click', '.edit-user', function () {
            const userId = $(this).data('userid');
            $('#editUserModal').modal('show');
            CommonMaster.loadModalContent('#editUserModalBody', `${userSettings.editModalUrl}?userId=${userId}`, '#editUserForm');
        });

        $(document).on('click', '.view-user', function () {
            const userId = $(this).data('userid');
            $('#viewUserModal').modal('show');
            CommonMaster.loadModalContent('#viewUserModalBody', `${userSettings.viewModalUrl}?userId=${userId}`);
        });

        $(document).on('click', '.delete-user', function () {
            const userId = $(this).data('userid');
            const userName = $(this).data('username');
            $('#itemToDeleteId').val(userId);
            $('#itemToDeleteName').text(userName);
            $('#deleteModal').modal('show');
        });
    }

    return { init };
})();

$(function () {
    const defaultSettings = {
        userMasterScreenUrl: window.location.pathname,
        userAddModalUrl: '/UserMaster/UserAddModal',
        userEditModalUrl: '/UserMaster/UserEditModal',
        userViewModalUrl: '/UserMaster/UserViewModal',
        deleteUserUrl: '/UserMaster/Delete'
    };

    const settings = typeof userMasterSettings !== 'undefined' ?
        { ...defaultSettings, ...userMasterSettings } : defaultSettings;

    UserMasterScreen.init(settings);
});
