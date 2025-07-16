function addToFavorites(bookId, title, coverImageUrl) {
    fetch('/Dashboard/AddToFavoritesBook', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ id: bookId })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showFavoriteMessage();
                updateFavoriteSidebar(title, coverImageUrl);
            } else {
                alert('Failed to add to favorites.');
            }
        })
        .catch(error => {
            console.error(error);
            alert('Error occurred while adding to favorites.');
        });
}

function showFavoriteMessage() {
    const alert = document.getElementById("favoriteAlert");
    alert.classList.remove("d-none");
    setTimeout(() => {
        alert.classList.add("d-none");
    }, 3000);
}

function hideFavoriteMessage() {
    document.getElementById("favoriteAlert").classList.add("d-none");
}

function updateFavoriteSidebar(title, coverImageUrl) {
    const sidebar = document.querySelector(".favorite-book-list");
    const emptyText = sidebar.querySelector("p");
    if (emptyText) emptyText.remove();

    const favoriteItem = document.createElement("div");
    favoriteItem.classList.add("favorite-book", "mb-2", "text-center");

    const image = document.createElement("img");
    image.src = coverImageUrl;
    image.alt = title;
    image.style.height = "80px";
    image.classList.add("mb-1");

    const name = document.createElement("p");
    name.classList.add("small");
    name.textContent = title;

    favoriteItem.appendChild(image);
    favoriteItem.appendChild(name);
    sidebar.appendChild(favoriteItem);
}
