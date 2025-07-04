$(document).ready(function () {

    function loadModalContent(modalId, targetUrl, targetBodyId) {
        $(modalId).on('show.bs.modal', function () {
            const $body = $(targetBodyId);
            $body.html('<div class="text-center py-3">Loading...</div>'); // Optional loading indicator
            $.ajax({
                url: targetUrl,
                method: 'GET',
                success: function (response) {
                    $body.html(response);
                },
                error: function () {
                    $body.html('<div class="text-danger text-center">Failed to load content.</div>');
                }
            });
        });
    }

    loadModalContent('#loginModal', '/LandingPage/Login', '#loginModalBody');
});