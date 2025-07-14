function showFavoriteMessage() {
    const alertBox = document.getElementById("favoriteAlert");
    alertBox.classList.remove("d-none");
    setTimeout(() => alertBox.classList.add("d-none"), 3000);
}

function hideFavoriteMessage() {
    document.getElementById("favoriteAlert").classList.add("d-none");
}

