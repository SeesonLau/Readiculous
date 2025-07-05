class PaginationGenreView {
    constructor() {
        this.genreId = document.querySelector('input[name="id"]').value;
        this.bookSearch = document.querySelector('input[name="bookSearch"]')?.value || '';
        this.initPagination();
        this.initJumpToPage();
    }

    initPagination() {
        document.querySelectorAll('.pagination-genre-link').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const page = e.currentTarget.getAttribute('data-page');
                if (page && !e.currentTarget.closest('.page-item').classList.contains('disabled')) {
                    this.navigateToPage(page);
                }
            });
        });
    }

    initJumpToPage() {
        const jumpToPageModal = document.getElementById('jumpToPageGenreModal');
        if (!jumpToPageModal) return;

        // When modal opens
        jumpToPageModal.addEventListener('show.bs.modal', () => {
            document.getElementById('pageNumberGenreInput').value = '';
            document.getElementById('pageNumberGenreError').textContent = '';
            document.getElementById('pageNumberGenreInput').classList.remove('is-invalid');
        });

        // Validate input
        document.getElementById('pageNumberGenreInput')?.addEventListener('input', (e) => {
            const value = parseInt(e.target.value);
            const max = parseInt(e.target.max);

            if (isNaN(value)) {
                e.target.classList.add('is-invalid');
                document.getElementById('pageNumberGenreError').textContent = 'Please enter a number';
                return;
            }

            if (value < 1 || value > max) {
                e.target.classList.add('is-invalid');
                document.getElementById('pageNumberGenreError').textContent = `Must be between 1 and ${max}`;
            } else {
                e.target.classList.remove('is-invalid');
                document.getElementById('pageNumberGenreError').textContent = '';
            }
        });

        // Handle confirmation
        document.getElementById('confirmJumpToGenrePage')?.addEventListener('click', () => {
            const input = document.getElementById('pageNumberGenreInput');
            const pageNumber = parseInt(input.value);
            const totalPages = parseInt(input.max);

            if (isNaN(pageNumber)) {
                input.classList.add('is-invalid');
                document.getElementById('pageNumberGenreError').textContent = 'Please enter a valid number';
                return;
            }

            if (pageNumber < 1 || pageNumber > totalPages) {
                input.classList.add('is-invalid');
                document.getElementById('pageNumberGenreError').textContent = `Please enter a number between 1 and ${totalPages}`;
                return;
            }

            bootstrap.Modal.getInstance(jumpToPageModal).hide();
            this.navigateToPage(pageNumber);
        });
    }

    navigateToPage(page) {
        let url = `${window.location.pathname}?id=${this.genreId}&page=${page}`;
        if (this.bookSearch) {
            url += `&bookSearch=${encodeURIComponent(this.bookSearch)}`;
        }
        window.location.href = url;
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new PaginationGenreView();
});