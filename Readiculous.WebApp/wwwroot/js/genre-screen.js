document.addEventListener("DOMContentLoaded", function () {
    const genreToggle = document.getElementById("genreToggle");
    const genreMenu = document.getElementById("genreMenu");
    const checkboxes = document.querySelectorAll(".genre-checkbox");
    const selectedGenresInput = document.getElementById("selectedGenresInput");

    // Toggle the dropdown
    genreToggle.addEventListener("click", function () {
        genreMenu.classList.toggle("show");
    });

    // Handle checkbox changes
    checkboxes.forEach(chk => {
        chk.addEventListener("change", function () {
            const selected = Array.from(checkboxes)
                .filter(c => c.checked)
                .map(c => c.value);

            // Update hidden input value
            selectedGenresInput.value = selected.join(",");

            // Update button label
            genreToggle.textContent = selected.length
                ? selected.join(", ")
                : "Select Genres…";

            // Fetch updated books
            fetch("/Dashboard/LoadBooksByGenres", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(selected)
            })
                .then(response => {
                    if (!response.ok) throw new Error("Failed to load books.");
                    return response.text();
                })
                .then(html => {
                    const container = document.getElementById("bookGridContainer");
                    if (container) {
                        container.innerHTML = html;
                    }
                })
                .catch(error => {
                    console.error(error);
                });
        });
    });
});
