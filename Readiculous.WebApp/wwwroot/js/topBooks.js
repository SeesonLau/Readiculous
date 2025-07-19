document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById('bookSearchTopBooks');
    const resultsContainer = document.getElementById('searchResultsTopBooks');
    const booksJson = document.getElementById('booksJsonTopBooks');
    const books = booksJson ? JSON.parse(booksJson.value) : [];

    searchInput.addEventListener('input', function () {
        const query = this.value.trim().toLowerCase();

        if (!query) {
            resultsContainer.style.display = 'none';
            resultsContainer.innerHTML = '';
            return;
        }

        const filtered = books.filter(b => b.title.toLowerCase().includes(query));

        if (filtered.length === 0) {
            resultsContainer.innerHTML = `
                <div class="list-group-item text-muted">
                    No books found.
                </div>`;
            resultsContainer.style.display = 'block';
            return;
        }

        resultsContainer.innerHTML = filtered.map(b => `
            <a href="/Dashboard/BookDetailScreen?id=${b.id}" class="list-group-item list-group-item-action">
                <img src="${b.cover}" alt="${b.title}" />
                <span>${b.title}</span>
            </a>
        `).join('');

        resultsContainer.style.display = 'block';
    });

    document.addEventListener('click', function (e) {
        if (!resultsContainer.contains(e.target) && e.target !== searchInput) {
            resultsContainer.style.display = 'none';
        }
    });
});
