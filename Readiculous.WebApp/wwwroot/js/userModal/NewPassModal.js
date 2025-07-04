function openNewPassModal() {
    fetch('/Auth/NewPassModalPartial')
        .then(response => response.text())
        .then(html => {
            document.getElementById('newPassModalBody').innerHTML = html;
            const modal = new bootstrap.Modal(document.getElementById('newPassModal'));
            modal.show();
        });
}
