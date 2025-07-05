// in genremasterView.js

document.addEventListener('DOMContentLoaded', function () {
    const genreViewModalElement = document.getElementById('genreViewModal');
    const genreViewModal = new bootstrap.Modal(genreViewModalElement);
    const genreViewModalBodyDiv = document.getElementById('genreViewModalBody'); // This div will receive the entire partial view content

    // Handle view button clicks
    document.addEventListener('click', function (e) {
        const viewBtn = e.target.closest('.view-genre-btn');
        if (viewBtn) {
            e.preventDefault();
            const genreId = viewBtn.dataset.genreId;

            // Optional: Show loading spinner immediately
            genreViewModalBodyDiv.innerHTML = '<div class="modal-content"><div class="modal-body text-center p-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div></div>';
            genreViewModal.show();

            // Fetch the content from the new partial-only action
            fetch(`${genreMasterSettings.genreViewModalUrl}?id=${genreId}`)
                .then(response => {
                    if (!response.ok) {
                        // Handle non-2xx responses from the controller (e.g., BadRequest)
                        return response.text().then(text => { throw new Error(text) });
                    }
                    return response.text();
                })
                .then(html => {
                    // Directly inject the entire HTML received from the partial view
                    genreViewModalBodyDiv.innerHTML = html;
                })
                .catch(error => {
                    console.error('Error loading modal:', error);
                    genreViewModalBodyDiv.innerHTML = `<div class="modal-content"><div class="modal-header"><h5 class="modal-title text-danger">Error</h5><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div><div class="modal-body"><p>Error loading genre details: ${error.message || 'An unexpected error occurred.'}</p></div><div class="modal-footer"><button type="button" class="btn genre-btn-pink" data-bs-dismiss="modal">Close</button></div></div>`;
                });
        }
    });

    // Handle search and pagination within the modal
    // Note: The form's asp-action should point to the new partial action as well.
    document.addEventListener('submit', function (e) {
        const form = e.target.closest('.genre-details-search-form');
        if (form) { // Check if the submitted form is within the modal's search form
            e.preventDefault();
            const formData = new FormData(form);
            const urlParams = new URLSearchParams(formData);

            fetch(`${genreMasterSettings.genreViewModalUrl}?${urlParams}`) // Use the new partial action URL
                .then(response => {
                    if (!response.ok) {
                        return response.text().then(text => { throw new Error(text) });
                    }
                    return response.text();
                })
                .then(html => {
                    genreViewModalBodyDiv.innerHTML = html; // Replace the whole modal content
                })
                .catch(error => {
                    console.error('Search error:', error);
                    // Decide how to show errors for search/pagination within the already open modal
                    // Perhaps update a specific error div inside the modal, or a toast.
                });
        }
    });

    document.addEventListener('click', function (e) {
        const pageLink = e.target.closest('.page-link');
        // Ensure the clicked link is specifically within the genreViewModal (important for pagination)
        if (pageLink && e.target.closest('#genreViewModal')) {
            e.preventDefault();
            const url = new URL(pageLink.href);

            fetch(`${genreMasterSettings.genreViewModalUrl}?${url.searchParams}`) // Use the new partial action URL
                .then(response => {
                    if (!response.ok) {
                        return response.text().then(text => { throw new Error(text) });
                    }
                    return response.text();
                })
                .then(html => {
                    genreViewModalBodyDiv.innerHTML = html; // Replace the whole modal content
                })
                .catch(error => {
                    console.error('Pagination error:', error);
                    // Same error handling consideration as search
                });
        }
    });

    // Optional: Clear modal content when closed
    genreViewModalElement.addEventListener('hidden.bs.modal', function (event) {
        genreViewModalBodyDiv.innerHTML = '';
    });
});
