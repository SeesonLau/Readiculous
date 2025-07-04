function openRegistrationSuccessModal() {
    fetch('/Auth/RegistrationSuccessModalPartial')
        .then(response => response.text())
        .then(html => {
            document.getElementById('registerSuccessModalBody').innerHTML = html;
            const modal = new bootstrap.Modal(document.getElementById('registerSuccessModal'));
            modal.show();
        });
}
