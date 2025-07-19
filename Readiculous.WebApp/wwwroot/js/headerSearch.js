(function () {
    const searchInput = document.getElementById('bookSearch');
    const resultsContainer = document.getElementById('searchResults');

    // Return early if elements don't exist
    if (!searchInput || !resultsContainer) return;

    const booksDataElement = document.getElementById('booksJson');
    const books = booksDataElement ? JSON.parse(booksDataElement.textContent) : [];

    searchInput.addEventListener('input', function () {
        const query = this.value.trim().toLowerCase();

        if (query.length === 0) {
            resultsContainer.style.display = 'none';
            resultsContainer.innerHTML = '';
            return;
        }

        const filtered = books.filter(b =>
            b.title.toLowerCase().includes(query) ||  // Changed to includes for better matching
            b.author?.toLowerCase().includes(query)  // Added author search
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
                    <img src="${b.cover || '/images/default-book-cover.png'}" 
                         alt="${b.title}"
                         style="width: 40px; height: 60px; object-fit: cover; border-radius: 4px; margin-right: 10px;" />
                    <div>
                        <div>${b.title}</div>
                        ${b.author ? `<small class="text-muted">${b.author}</small>` : ''}
                    </div>
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