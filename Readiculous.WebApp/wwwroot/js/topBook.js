(function () {
    const searchInput = document.getElementById('bookSearchTopBooks');
    const resultsContainer = document.getElementById('searchResultsTopBooks');
    const booksDataElement = document.getElementById('booksJsonTopBooks');

    const books = booksDataElement ? JSON.parse(booksDataElement.value) : [];

    searchInput.addEventListener('input', function () {
        const query = this.value.trim().toLowerCase();

        if (query.length === 0) {
            resultsContainer.style.display = 'none';
            resultsContainer.innerHTML = '';
            return;
        }

        const filtered = books.filter(b =>
            b.title.toLowerCase().startsWith(query)
        );

        if (filtered.length === 0) {
            resultsContainer.innerHTML = `
                <div class="list-group-item text-muted">
                    No books found
                </div>`;
            resultsContainer.style.display = 'block';
            return;
        }

        let html = '';
        filtered.forEach(b => {
            html += `
                <a href="/Dashboard/BookDetailScreen?id=${b.id}"
                   class="list-group-item list-group-item-action d-flex align-items-center">
                    <img src="${b.cover}" alt="${b.title}" />
                    <span>${b.title}</span>
                </a>`;
        });

        resultsContainer.innerHTML = html;
        resultsContainer.style.display = 'block';
    });

    document.addEventListener('click', function (e) {
        if (!resultsContainer.contains(e.target) && e.target !== searchInput) {
            resultsContainer.style.display = 'none';
        }
    });
})();
