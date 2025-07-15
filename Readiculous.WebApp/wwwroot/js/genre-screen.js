document.addEventListener("DOMContentLoaded", function () {
    const checkboxes = document.querySelectorAll(".genre-checkbox");
    const selectedGenresInput = document.getElementById("selectedGenresInput");
    const bookContainer = document.querySelector(".section-box");
    const genreTitle = document.querySelector(".genre-title-bordered");
    const warningMessage = document.querySelector(".alert-warning");

    checkboxes.forEach(cb => {
        cb.addEventListener("change", function () {
            const selected = Array.from(checkboxes)
                .filter(c => c.checked)
                .map(c => c.value);

            selectedGenresInput.value = selected.join(",");

            // Fetch books for selected genres
            fetchBooksByGenre(selected);
        });
    });

    function fetchBooksByGenre(genres) {
        fetch(`/Genre/GetBooksByGenres?genres=${encodeURIComponent(genres.join(","))}`)
            .then(res => res.json())
            .then(data => {
                // Clear current books
                bookContainer.innerHTML = "";

                if (data.books.length > 0) {
                    data.books.forEach(book => {
                        const filledStars = Math.floor(book.averageRating);
                        const hasReview = book.averageRating > 0;
                        const starsHtml = Array.from({ length: 5 }, (_, i) => {
                            return `<span class="star-icon ${i < filledStars && hasReview ? 'star-filled' : 'star-empty'}">☆</span>`;
                        }).join("");

                        const html = `
                        <div class="col">
                            <a href="/Dashboard/BookDetailScreen/${book.bookId}" class="text-decoration-none text-reset">
                                <div class="book-card h-100 text-center">
                                    <img src="${book.coverImageUrl || "/img/placeholder.png"}" class="book-image mb-3 img-fluid"
                                        alt="${book.title}" style="height: 220px; object-fit: contain;" />
                                    <h6 class="book-title mb-1">${book.title}</h6>
                                    <p class="book-author mb-1 small">${book.author}, ${book.publicationYear}</p>
                                    <div class="book-rating mb-0">
                                        ${starsHtml}
                                        <span class="rating-value d-block ${hasReview ? "text-warning" : "text-secondary"}">
                                            (${book.averageRating.toFixed(1)})
                                        </span>
                                        <span class="review-count small text-muted d-block">
                                            (${book.totalReviews} review${book.totalReviews === 1 ? "" : "s"})
                                        </span>
                                    </div>
                                </div>
                            </a>
                        </div>`;
                        bookContainer.insertAdjacentHTML("beforeend", html);
                    });

                    genreTitle.textContent = genres.join(", ").toUpperCase();
                    genreTitle.parentElement.style.display = "block";
                    if (warningMessage) warningMessage.style.display = "none";
                } else {
                    genreTitle.parentElement.style.display = "none";
                    if (warningMessage) warningMessage.style.display = "block";
                }
            })
            .catch(err => {
                console.error("Error fetching books:", err);
            });
    }
});
