function openSuccessfullyNewPassModal() {
    fetch('/Auth/SuccessfullyNewPassModalPartial')
        .then(response => response.text())
        .then(html => {
            document.getElementById('successPassModalBody').innerHTML = html;
            const modal = new bootstrap.Modal(document.getElementById('successPassModal'));
            modal.show();
        });
}
