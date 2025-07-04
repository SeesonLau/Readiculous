function openOTPModal() {
    fetch('/Auth/OTPModalPartial')
        .then(response => response.text())
        .then(html => {
            document.getElementById('otpModalBody').innerHTML = html;
            const modal = new bootstrap.Modal(document.getElementById('otpModal'));
            modal.show();
        });
}
