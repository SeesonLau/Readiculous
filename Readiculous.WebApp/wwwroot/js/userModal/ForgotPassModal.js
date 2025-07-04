function openForgotPassModal() {
    fetch('/Auth/ForgotPassModalPartial')
        .then(response => response.text())
        .then(html => {
            document.getElementById('forgotPassModalBody').innerHTML = html;
            const modal = new bootstrap.Modal(document.getElementById('forgotPassModal'));
            modal.show();
        });
}
