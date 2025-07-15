document.addEventListener("DOMContentLoaded", function () {
    const dashboardInput = document.getElementById('searchDashboard');
    const dashboardSuggestions = document.getElementById('searchSuggestions');
    const dashboardJson = document.getElementById('booksJsonDashboard');
    const dashboardBooks = dashboardJson ? JSON.parse(dashboardJson.value) : [];

    const newBooksJson = document.getElementById('booksJsonNewBooks');
    const newBooks = newBooksJson ? JSON.parse(newBooksJson.value) : [];

    const topBooksInput = document.getElementById('bookSearchTopBooks');
    const topBooksResults = document.getElementById('searchResultsTopBooks');
    const topBooksJson = document.getElementById('booksJsonTopBooks');
    const topBooksData = topBooksJson ? JSON.parse(topBooksJson.value) : [];

    // 🔍 DASHBOARD + NEW BOOKS local search
    const setupLocalSearch = (input, suggestions, books, sortType = "createdTime") => {
        if (!input || !suggestions || !books.length) return;

        input.addEventListener('input', function () {
            const query = input.value.trim().toLowerCase();
            suggestions.innerHTML = '';
            if (!query) {
                suggestions.style.display = 'none';
                return;
            }

            let filtered = books.filter(b => b.title.toLowerCase().includes(query));

            if (sortType === "createdTime") {
                filtered.sort((a, b) => new Date(b.createdTime) - new Date(a.createdTime));
            } else if (sortType === "averageRating") {
                filtered.sort((a, b) => b.averageRating - a.averageRating);
            }

            if (filtered.length === 0) {
                suggestions.innerHTML = '<li class="list-group-item text-muted text-center">No book found</li>';
                suggestions.style.display = 'block';
                return;
            }

            filtered.slice(0, 10).forEach(book => {
                const item = document.createElement('li');
                item.classList.add('list-group-item', 'd-flex', 'align-items-center');
                item.style.cursor = 'pointer';
                item.innerHTML = `
                    <img src="${book.cover}" alt="${book.title}" style="width: 40px; height: 60px; object-fit: cover; margin-right: 10px;">
                    <span>${book.title}</span>
                `;
                item.addEventListener('click', () => {
                    window.location.href = `/Dashboard/BookDetailScreen/${book.id}`;
                });
                suggestions.appendChild(item);
            });

            suggestions.style.display = 'block';
        });

        document.addEventListener('click', (e) => {
            if (!suggestions.contains(e.target) && e.target !== input) {
                suggestions.style.display = 'none';
            }
        });
    };

    // Apply it
    setupLocalSearch(dashboardInput, dashboardSuggestions, dashboardBooks, "createdTime");
    setupLocalSearch(document.getElementById('searchDashboard'), dashboardSuggestions, newBooks, "createdTime");
    setupLocalSearch(topBooksInput, topBooksResults, topBooksData, "averageRating");
});
