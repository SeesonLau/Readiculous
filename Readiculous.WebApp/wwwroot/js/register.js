
document.addEventListener("DOMContentLoaded", function () {
    const registerForm = document.getElementById("registerForm");
    const registerError = document.getElementById("registerError");
    const registerModal = document.getElementById("registerModal");
    const otpModal = document.getElementById("otpModal");
    const otpEmailInput = document.getElementById("otpEmail");
    const otpEmailDisplay = document.getElementById("otpEmailDisplay");

    if (registerForm) {
        registerForm.addEventListener("submit", function (e) {
            e.preventDefault();

            // Get the email value
            const email = registerForm.querySelector("input[name='Email']").value.trim();
            if (!email) {
                registerError.innerText = "Please enter your email.";
                registerError.style.display = "block";
                return;
            }

            registerError.style.display = "none";
            registerError.innerText = "";

            // Show loading state on button
            const submitBtn = registerForm.querySelector("button[type='submit']");
            submitBtn.disabled = true;
            submitBtn.innerText = "Processing...";

            // Send request to backend to trigger OTP
            fetch("/Account/RequestOtp", {
                method: "POST",
                body: new URLSearchParams({ Email: email }),
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                credentials: "same-origin"
            })
                .then(res => {
                    if (res.redirected) {
                        // If redirected, it means we're going to the OTP page
                        // For modal flow, we'll handle this differently
                        return res.text();
                    }
                    return res.json();
                })
                .then(data => {
                    if (data && data.success !== undefined) {
                        // JSON response (error case)
                        if (data.success) {
                            // Close Register Modal
                            const regModalInstance = bootstrap.Modal.getInstance(registerModal);
                            if (regModalInstance) regModalInstance.hide();

                            // Fill OTP modal with email
                            otpEmailInput.value = email;
                            otpEmailDisplay.innerText = email;

                            // Show OTP Modal
                            const otpModalInstance = new bootstrap.Modal(otpModal);
                            otpModalInstance.show();
                        } else {
                            registerError.innerText = data.message || "Could not send OTP.";
                            registerError.style.display = "block";
                        }
                    } else {
                        // HTML response (success case - page redirect)
                        // For modal flow, we'll simulate the success
                        const regModalInstance = bootstrap.Modal.getInstance(registerModal);
                        if (regModalInstance) regModalInstance.hide();

                        // Fill OTP modal with email
                        otpEmailInput.value = email;
                        otpEmailDisplay.innerText = email;

                        // Show OTP Modal
                        const otpModalInstance = new bootstrap.Modal(otpModal);
                        otpModalInstance.show();
                    }
                })
                .catch(err => {
                    console.error(err);
                    registerError.innerText = "Something went wrong.";
                    registerError.style.display = "block";
                })
                .finally(() => {
                    submitBtn.disabled = false;
                    submitBtn.innerText = "Sign Up";
                });
        });
    }

    // OTP Form handling
    const otpForm = document.getElementById("otpForm");
    const otpError = document.getElementById("otpError");
    const resendOtpLink = document.getElementById("resendOtpLink");

    if (otpForm) {
        // OTP input logic: auto-advance, backspace, and collect value
        const inputs = document.querySelectorAll('.otp-input');
        const hiddenOtp = document.getElementById('OtpHidden');

        if (inputs.length > 0) {
            inputs[0].focus();
            inputs.forEach((input, idx) => {
                input.addEventListener('input', function (e) {
                    if (this.value.length > 1) this.value = this.value.slice(0, 1);
                    if (this.value && idx < inputs.length - 1) {
                        inputs[idx + 1].focus();
                    }
                    updateHiddenOtp();
                });
                input.addEventListener('keydown', function (e) {
                    if (e.key === 'Backspace' && !this.value && idx > 0) {
                        inputs[idx - 1].focus();
                    }
                });
            });
        }

        function updateHiddenOtp() {
            let otp = '';
            inputs.forEach(i => otp += i.value);
            hiddenOtp.value = otp;
        }

        otpForm.addEventListener("submit", function (e) {
            e.preventDefault();
            updateHiddenOtp();

            otpError.style.display = "none";
            otpError.innerText = "";

            const formData = new FormData(otpForm);
            const submitBtn = otpForm.querySelector("button[type='submit']");
            submitBtn.disabled = true;
            submitBtn.innerText = "Verifying...";

            fetch("/Account/VerifyOtp", {
                method: "POST",
                body: formData
            })
                .then(res => {
                    if (res.redirected) {
                        // Success - show All Done modal instead of redirecting
                        const otpModalInstance = bootstrap.Modal.getInstance(otpModal);
                        if (otpModalInstance) otpModalInstance.hide();

                        // Show All Done modal
                        showAllDoneModal("Registration successful! Check your email for your temporary password.");
                        return;
                    }
                    return res.json();
                })
                .then(data => {
                    if (data && data.success !== undefined) {
                        // JSON response (error case)
                        if (data.success) {
                            // Close OTP modal and show All Done modal
                            const otpModalInstance = bootstrap.Modal.getInstance(otpModal);
                            if (otpModalInstance) otpModalInstance.hide();

                            // Show All Done modal
                            showAllDoneModal("Registration successful! Check your email for your temporary password.");
                        } else {
                            otpError.innerText = data.message || "Invalid OTP. Please try again.";
                            otpError.style.display = "block";
                        }
                    }
                })
                .catch(err => {
                    console.error(err);
                    otpError.innerText = "An error occurred.";
                    otpError.style.display = "block";
                })
                .finally(() => {
                    submitBtn.disabled = false;
                    submitBtn.innerText = "Continue";
                });
        });
    }

    // Resend OTP functionality
    if (resendOtpLink) {
        resendOtpLink.addEventListener('click', function (e) {
            e.preventDefault();
            const email = otpEmailInput.value;

            fetch('/Account/ResendOtp', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: `email=${encodeURIComponent(email)}`
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        toastr.success('New OTP has been sent to your email address.');
                    } else {
                        toastr.error('Failed to resend OTP. Please try again.');
                    }
                })
                .catch(error => {
                    toastr.error('An error occurred. Please try again.');
                });
        });
    }
});

