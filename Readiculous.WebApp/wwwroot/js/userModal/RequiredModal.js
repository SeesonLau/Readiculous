function openRequiredModal() {
    fetch('/Auth/RequiredModalPartial')
        .then(response => response.text())
        .then(html => {
            document.getElementById('requiredModalBody').innerHTML = html;
            const modal = new bootstrap.Modal(document.getElementById('requiredModal'));
            modal.show();
        });
}
