function openEditUserModal(userId) {
    fetch(`/User/EditUserModal?userId=${userId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error("Failed to load modal");
            }
            return response.text();
        })
        .then(html => {
            const modalContent = document.getElementById('modalContent');
            modalContent.innerHTML = html;

            const modalElement = document.getElementById('editUserModal');
            const modal = new bootstrap.Modal(modalElement);
            modal.show();
        })
        .catch(err => console.error("Error loading edit user modal:", err));
}

document.body.addEventListener('submit', async function (e) {
    if (e.target && e.target.id === 'editUserForm') {
        e.preventDefault();
        const form = e.target;
        const formData = new FormData(form);

        try {
            const response = await fetch('/User/EditAsync', {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.redirected) {
                window.location.href = response.url;
            } else {
                const html = await response.text();
                document.getElementById('modalContent').innerHTML = html;
            }
        } catch (err) {
            console.error("Failed to submit edit form:", err);
        }
    }
});