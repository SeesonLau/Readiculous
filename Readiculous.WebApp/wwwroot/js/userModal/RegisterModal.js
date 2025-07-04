function openRegisterModal() {
    fetch('/Auth/RegisterModalPartial')
        .then(response => response.text())
        .then(html => {
            document.getElementById('registerModalBody').innerHTML = html;
            const modal = new bootstrap.Modal(document.getElementById('registerModal'));
            modal.show();
        });
}
