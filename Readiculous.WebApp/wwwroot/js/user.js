// Function to open the modal and load its content
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

            // Rebind validation for the newly loaded form content
            const editForm = document.getElementById('editUserForm');
            if (editForm) {
                // $.validator is now available because of the fix in Index.cshtml
                $.validator.unobtrusive.parse(editForm);
            }
        })
        .catch(err => console.error("Error loading edit user modal:", err));
}

document.addEventListener('DOMContentLoaded', function () {
    document.body.addEventListener('submit', async function (e) {
        const form = e.target;
        if (form && form.id === 'editUserForm') {
            e.preventDefault();

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

                    // Re-apply validation
                    $.validator.unobtrusive.parse('#editUserForm');
                }
            } catch (err) {
                console.error("Error submitting edit user form:", err);
            }
        }
    });
});



// Attach a single, delegated event listener to the body
document.addEventListener('DOMContentLoaded', function () {
    document.body.addEventListener('submit', function (e) {
        // Check if the submitted form is the one we want to handle
        if (e.target && e.target.id === 'editUserForm') {
            e.preventDefault();
        }
    });
});

function openRegistrationModal(userId) {
    $.ajax({
        url: '/User/Details', // Matches your controller
        type: 'GET',
        data: { userId: userId },
        success: function (result) {
            $('#registrationModalContent').html(result); // Inject the partial view
            const modal = new bootstrap.Modal(document.getElementById('registrationModal'));
            modal.show(); // Show the modal
        },
        error: function () {
            alert('Failed to load user details.');
        }
    });
}
