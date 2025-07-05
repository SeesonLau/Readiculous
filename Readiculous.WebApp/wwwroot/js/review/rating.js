window.setRating = function (value) {
    const ratingInput = document.getElementById('Rating');
    const stars = document.querySelectorAll('#ratingWidget i');

    ratingInput.value = value;

    stars.forEach(star => {
        const starValue = parseInt(star.getAttribute('data-value'));
        if (starValue <= value) {
            star.classList.remove('bi-star', 'text-secondary');
            star.classList.add('bi-star-fill', 'text-warning');
        } else {
            star.classList.remove('bi-star-fill', 'text-warning');
            star.classList.add('bi-star', 'text-secondary');
        }
    });
}