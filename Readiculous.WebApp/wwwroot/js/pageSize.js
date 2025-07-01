document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.page-size-container button').forEach(button => {
        button.addEventListener('click', function () {
            const pageSize = this.getAttribute('data-page-size');
            updatePageSize(pageSize);
        });
    });
});

function updatePageSize(pageSize) {
    const url = new URL(window.location.href);
    url.searchParams.set('pageSize', pageSize);
    url.searchParams.set('page', '1'); // Reset to first page when changing page size
    window.location.href = url.toString();
}
