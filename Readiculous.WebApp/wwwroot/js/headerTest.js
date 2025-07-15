document.addEventListener("DOMContentLoaded", function () {
    const profileIcon = document.getElementById("profileIcon");

    if (profileIcon) {
        profileIcon.addEventListener("click", function () {
            const modal = new bootstrap.Modal(document.getElementById('editProfileModal'));
            modal.show();
        });
    }
});
