otpForm.addEventListener("submit", function (e) {
    e.preventDefault();
    otpError.style.display = "none";

    const formData = new FormData(otpForm);
    const submitBtn = otpForm.querySelector("button[type='submit']");
    submitBtn.disabled = true;
    submitBtn.innerText = "Verifying...";

    fetch("/Account/VerifyOtpAjax", {
        method: "POST",
        body: formData
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                // ✅ HIDE OTP modal
                const otpModal = bootstrap.Modal.getInstance(document.getElementById("otpModal"));
                if (otpModal) otpModal.hide();

                // ✅ Call separate function to handle success flow
                showRegistrationSuccessModal();
            } else {
                otpError.innerText = data.message;
                otpError.style.display = "block";
            }
        })
        .catch(err => {
            otpError.innerText = "An error occurred.";
            otpError.style.display = "block";
        })
        .finally(() => {
            submitBtn.disabled = false;
            submitBtn.innerText = "Verify";
        });
});
