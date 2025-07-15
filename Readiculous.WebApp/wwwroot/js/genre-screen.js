document.addEventListener("DOMContentLoaded", function () {
    const genreCheckboxes = document.querySelectorAll(".genre-checkbox");
    const sortSelect = document.getElementById("sortTypeSelect");
    const bookResults = document.getElementById("bookResults");
    const selectedGenresInput = document.getElementById("selectedGenresInput");

    function loadBooks() {
        const selectedGenres = Array.from(genreCheckboxes)
            .filter(c => c.checked)
            .map(c => c.value)
            .join(",");

        selectedGenresInput.value = selectedGenres;

        const sortType = sortSelect.value;

        const url = `/Dashboard/GenreBooksPartial?selectedGenres=${encodeURIComponent(selectedGenres)}&sortType=${encodeURIComponent(sortType)}`;

        fetch(url, { method: "GET" })
            .then(response => response.text())
            .then(html => {
                bookResults.innerHTML = html;
            });
    }

    genreCheckboxes.forEach(c => {
        c.addEventListener("change", loadBooks);
    });

    sortSelect.addEventListener("change", loadBooks);
});
