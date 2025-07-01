document.addEventListener('DOMContentLoaded', function () {
    // Get current URL parameters
    // Pagination updates as URL updates itself
    const urlParams = new URLSearchParams(window.location.search);

    // Main function to update URL and reload
    function updateUrlParams(params) {
        const url = new URL(window.location.href);

        // Update or add parameters
        Object.keys(params).forEach(key => {
            if (params[key] !== null && params[key] !== undefined && params[key] !== '') {
                url.searchParams.set(key, params[key]);
            } else {
                url.searchParams.delete(key);
            }
        });

        // Always reset to page 1 when search/filter changes (unless page is explicitly set)
        if (!params.hasOwnProperty('page')) {
            url.searchParams.set('page', '1');
        }

        window.location.href = url.toString();
    }

    // Debounce function to prevent rapid firing of search events
    function debounce(func, timeout = 500) {
        let timer;
        return (...args) => {
            clearTimeout(timer);
            timer = setTimeout(() => { func.apply(this, args); }, timeout);
        };
    }

    // Handle search input with debounce
    const searchInput = document.getElementById('searchString');
    if (searchInput) {
        searchInput.addEventListener('input', debounce(function (e) {
            updateUrlParams({
                searchString: e.target.value.trim(),
                page: null
            });
        }));
    }

    // Handle role type filter
    const roleTypeSelect = document.getElementById('roleType');
    if (roleTypeSelect) {
        roleTypeSelect.addEventListener('change', function () {
            updateUrlParams({
                roleType: this.value || null,
                page: null
            });
        });
    }

    // Handle sort type
    const searchTypeSelect = document.getElementById('searchType');
    if (searchTypeSelect) {
        searchTypeSelect.addEventListener('change', function () {
            updateUrlParams({
                searchType: this.value,
                page: null
            });
        });
    }

    // Handle pagination clicks
    document.querySelectorAll('.pagination .page-link:not(.jump-to-page)').forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            const page = this.getAttribute('data-page');
            if (page && !this.closest('.page-item').classList.contains('disabled')) {
                updateUrlParams({ page });
            }
        });
    });

    // Handle page size changes
    document.querySelectorAll('[data-page-size]').forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            updateUrlParams({
                pageSize: this.getAttribute('data-page-size'),
                page: '1'
            });
        });
    });

    // Jump to Page Modal Handling
    const jumpToPageModal = document.getElementById('jumpToPageModal');
    if (jumpToPageModal) {
        // When modal opens
        jumpToPageModal.addEventListener('show.bs.modal', function () {
            const currentDisplay = document.querySelector('.jump-to-page').textContent;
            const totalPages = parseInt(currentDisplay.split(' of ')[1]);

            document.getElementById('pageNumberInput').value = '';
            document.getElementById('pageNumberInput').max = totalPages;
            document.getElementById('pageNumberError').textContent = '';
            document.getElementById('pageNumberInput').classList.remove('is-invalid');
        });

        // Validate input on the fly
        document.getElementById('pageNumberInput')?.addEventListener('input', function () {
            const value = parseInt(this.value);
            const max = parseInt(this.max);

            if (isNaN(value)) {
                this.classList.add('is-invalid');
                document.getElementById('pageNumberError').textContent = 'Please enter a number';
                return;
            }

            if (value < 1 || value > max) {
                this.classList.add('is-invalid');
                document.getElementById('pageNumberError').textContent = `Must be between 1 and ${max}`;
            } else {
                this.classList.remove('is-invalid');
                document.getElementById('pageNumberError').textContent = '';
            }
        });

        // Handle jump confirmation
        document.getElementById('confirmJumpToPage').addEventListener('click', function () {
            const input = document.getElementById('pageNumberInput');
            const pageNumber = parseInt(input.value);
            const totalPages = parseInt(input.max);

            if (isNaN(pageNumber)) {
                input.classList.add('is-invalid');
                document.getElementById('pageNumberError').textContent = 'Please enter a valid number';
                return;
            }

            if (pageNumber < 1 || pageNumber > totalPages) {
                input.classList.add('is-invalid');
                document.getElementById('pageNumberError').textContent = `Please enter a number between 1 and ${totalPages}`;
                return;
            }

            const modal = bootstrap.Modal.getInstance(jumpToPageModal);
            modal.hide();
            updateUrlParams({ page: pageNumber });
        });
    }

    // Initialize form controls with current values
    function initializeFormControls() {
        // Set search input value
        if (searchInput && urlParams.has('searchString')) {
            searchInput.value = urlParams.get('searchString');
        }

        // Set role type select value
        if (roleTypeSelect && urlParams.has('roleType')) {
            roleTypeSelect.value = urlParams.get('roleType');
        }

        // Set search type select value
        if (searchTypeSelect && urlParams.has('searchType')) {
            searchTypeSelect.value = urlParams.get('searchType');
        }
    }

    initializeFormControls();
});
