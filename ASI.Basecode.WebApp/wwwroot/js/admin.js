// Run once DOM is fully loaded
document.addEventListener("DOMContentLoaded", () => {
    // === Profile Dropdown ===
    const profileIcon = document.querySelector(".profile-icon");
    const dropdownMenu = document.getElementById("profileMenu");

    if (profileIcon && dropdownMenu) {
        profileIcon.addEventListener("click", toggleDropdown);

        // Close dropdown when clicking outside
        document.addEventListener("click", function (e) {
            if (!e.target.closest(".profile-dropdown")) {
                dropdownMenu.classList.add("hidden");
                dropdownMenu.classList.remove("show");
            }
        });
    }

    // === Live Search Filter ===
    const searchInput = document.querySelector(".admin-search");
    const rows = document.querySelectorAll(".admin-book-table tbody tr");

    if (searchInput) {
        searchInput.addEventListener("input", () => {
            const query = searchInput.value.toLowerCase();
            rows.forEach(row => {
                const text = row.innerText.toLowerCase();
                row.style.display = text.includes(query) ? "" : "none";
            });
        });
    }

    // === Auto-close modal when escape is pressed
    document.addEventListener("keydown", (e) => {
        if (e.key === "Escape") {
            closeDeleteModal();
        }
    });
});

// === File Preview for Cover Image Upload
function previewFile(input) {
    const preview = document.getElementById('previewImage');
    const file = input.files[0];
    const reader = new FileReader();

    reader.onload = () => {
        preview.src = reader.result;
    };

    if (file) {
        reader.readAsDataURL(file);
    }
}

// === Toggle Profile Dropdown
function toggleDropdown(event) {
    event.stopPropagation();
    const menu = document.getElementById("profileMenu");
    menu.classList.toggle("hidden");
    menu.classList.toggle("show");
}

// === Delete Modal Logic
let bookIdToDelete = null;

function showDeleteModal(bookId) {
    bookIdToDelete = bookId;
    const authModal = document.getElementById("authModal");
    const deleteModal = document.getElementById("deleteModal");

    if (authModal && deleteModal) {
        authModal.classList.remove("hidden");
        setTimeout(() => authModal.classList.add("show"), 10);

        document.querySelectorAll(".auth-box").forEach(box => box.classList.add("hidden"));
        deleteModal.classList.remove("hidden");
    }
}

function closeDeleteModal() {
    const authModal = document.getElementById("authModal");
    if (authModal) {
        authModal.classList.remove("show");
        setTimeout(() => authModal.classList.add("hidden"), 200);
    }
    bookIdToDelete = null;
}

function confirmDelete() {
    if (bookIdToDelete !== null) {
        fetch(`/Admin/DeleteBook`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: `id=${bookIdToDelete}`
        })
            .then(response => {
                if (response.ok) {
                    const row = document.getElementById("book-row-" + bookIdToDelete);
                    if (row) row.remove();
                    closeDeleteModal();
                }
            });

        bookIdToDelete = null;
    }
}
