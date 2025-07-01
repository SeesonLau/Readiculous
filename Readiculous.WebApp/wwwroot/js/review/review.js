function openAddReviewModal(bookId, bookTitle) {
    fetch(`/Book/CreateReview?id=${bookId}`)
        .then(response => response.text())
        .then(html => {
            document.getElementById('reviewModalContent').innerHTML = html;
            let modal = new bootstrap.Modal(document.getElementById('addReviewModal'));
            modal.show();
        });
}

document.addEventListener('DOMContentLoaded', function () {
    document.body.addEventListener('submit', async function (e) {
        if (e.target && e.target.id === 'addReviewForm') {
            e.preventDefault();

            const form = e.target;
            const formData = new FormData(form);

            try {
                const response = await fetch('/Book/CreateReview', {
                    method: 'POST',
                    body: formData,
                });

                if (response.redirected) {
                    window.location.href = response.url; 
                } else {
                    const newHtml = await response.text();
                    document.getElementById('reviewModalContent').innerHTML = newHtml;
                }
            } catch (err) {
                console.error("Error submitting review:", err);
            }
        }
    });
});

function openEditReviewModal(bookId) {
    fetch(`/Book/EditReviewModal?id=${bookId}`)
        .then(response => response.text())
        .then(html => {
            document.getElementById('reviewModalContent').innerHTML = html;
            const modal = new bootstrap.Modal(document.getElementById('addReviewModal'));
            modal.show();
        })
        .catch(error => {
            console.error("Failed to load edit review modal:", error);
        });
}

document.addEventListener('DOMContentLoaded', function () {
    document.body.addEventListener('submit', async function (e) {
        if (e.target && (e.target.id === 'editReviewForm' || e.target.id === 'addReviewForm')) {
            e.preventDefault();

            const form = e.target;
            const formData = new FormData(form);

            try {
                const response = await fetch(form.id === 'editReviewForm' ? '/Book/EditReview' : '/Book/CreateReview', {
                    method: 'POST',
                    body: formData,
                });

                if (response.redirected) {
                    window.location.href = response.url;
                } else {
                    const newHtml = await response.text();
                    document.getElementById('reviewModalContent').innerHTML = newHtml;
                }
            } catch (err) {
                console.error("Error submitting review:", err);
            }
        }
    });
});
