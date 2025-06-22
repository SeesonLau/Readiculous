// Run once DOM is fully loaded
document.addEventListener("DOMContentLoaded", () => {
    // Profile Dropdown Toggle
    const profileIcon = document.querySelector(".profile-icon");
    const dropdownMenu = document.getElementById("profileMenu");

    if (profileIcon && dropdownMenu) {
        profileIcon.addEventListener("click", toggleDropdown);

        // Hide dropdown when clicking outside
        document.addEventListener("click", function (e) {
            if (!e.target.closest(".profile-dropdown")) {
                dropdownMenu.classList.add("hidden");
                dropdownMenu.classList.remove("show");
            }
        });
    }

    // Search Filter Logic
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
});

// File preview function (image upload)
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

// Toggle Profile Dropdown
function toggleDropdown(event) {
    event.stopPropagation();
    const menu = document.getElementById("profileMenu");
    menu.classList.toggle("hidden");
    menu.classList.toggle("show");
}

let selectedDeleteId = null;

function showDeleteModal(id) {
    selectedDeleteId = id;
    document.getElementById("authModal").classList.remove("hidden");
}

function closeDeleteModal() {
    selectedDeleteId = null;
    document.getElementById("authModal").classList.add("hidden");
}

function confirmDelete() {
    if (selectedDeleteId !== null) {
        const form = document.getElementById("deleteForm");
        document.getElementById("deleteBookId").value = selectedDeleteId;
        form.submit();
    }
}
