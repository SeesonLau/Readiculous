document.addEventListener("DOMContentLoaded", function () {
    // Forgot Password Form handling
    const forgotPasswordForm = document.getElementById("forgotPasswordForm");
    const forgotPasswordError = document.getElementById("forgotPasswordError");
    const forgotPasswordModal = document.getElementById("forgotPasswordModal");
    const forgotPasswordOtpModal = document.getElementById("forgotPasswordOtpModal");
    const forgotOtpEmailInput = document.getElementById("forgotOtpEmail");
    const forgotOtpEmailDisplay = document.getElementById("forgotOtpEmailDisplay");

    if (forgotPasswordForm) {
        forgotPasswordForm.addEventListener("submit", function (e) {
            e.preventDefault();

            // Get the email value
            const email = forgotPasswordForm.querySelector("input[name='Email']").value.trim();
            if (!email) {
                forgotPasswordError.innerText = "Please enter your email.";
                forgotPasswordError.style.display = "block";
                return;
            }

            forgotPasswordError.style.display = "none";
            forgotPasswordError.innerText = "";

            // Show loading state on button
            const submitBtn = forgotPasswordForm.querySelector("button[type='submit']");
            submitBtn.disabled = true;
            submitBtn.innerText = "Sending...";

            // Send request to backend to trigger OTP
            fetch("/Account/RequestForgotPasswordOtp", {
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
                        // Close Forgot Password Modal
                        const forgotModalInstance = bootstrap.Modal.getInstance(forgotPasswordModal);
                        if (forgotModalInstance) forgotModalInstance.hide();

                        // Fill OTP modal with email
                        forgotOtpEmailInput.value = email;
                        forgotOtpEmailDisplay.innerText = email;

                        // Show OTP Modal
                        const otpModalInstance = new bootstrap.Modal(forgotPasswordOtpModal);
                        otpModalInstance.show();
                    } else {
                        forgotPasswordError.innerText = data.message || "Could not send OTP.";
                        forgotPasswordError.style.display = "block";
                    }
                } else {
                    // HTML response (success case - page redirect)
                    // For modal flow, we'll simulate the success
                    const forgotModalInstance = bootstrap.Modal.getInstance(forgotPasswordModal);
                    if (forgotModalInstance) forgotModalInstance.hide();

                    // Fill OTP modal with email
                    forgotOtpEmailInput.value = email;
                    forgotOtpEmailDisplay.innerText = email;

                    // Show OTP Modal
                    const otpModalInstance = new bootstrap.Modal(forgotPasswordOtpModal);
                    otpModalInstance.show();
                }
            })
            .catch(err => {
                console.error(err);
                forgotPasswordError.innerText = "Something went wrong.";
                forgotPasswordError.style.display = "block";
            })
            .finally(() => {
                submitBtn.disabled = false;
                submitBtn.innerText = "Send Code";
            });
        });
    }

    // Forgot Password OTP Form handling
    const forgotPasswordOtpForm = document.getElementById("forgotPasswordOtpForm");
    const forgotOtpError = document.getElementById("forgotOtpError");
    const resendForgotOtpLink = document.getElementById("resendForgotOtpLink");
    const resetPasswordModal = document.getElementById("resetPasswordModal");
    const resetPasswordEmailInput = document.getElementById("resetPasswordEmail");

    if (forgotPasswordOtpForm) {
        // OTP input logic: auto-advance, backspace, and collect value
        const inputs = document.querySelectorAll('#forgotPasswordOtpModal .otp-input');
        const hiddenOtp = document.getElementById('ForgotOtpHidden');
        
        if (inputs.length > 0) {
            inputs[0].focus();
            inputs.forEach((input, idx) => {
                input.addEventListener('input', function(e) {
                    if (this.value.length > 1) this.value = this.value.slice(0, 1);
                    if (this.value && idx < inputs.length - 1) {
                        inputs[idx + 1].focus();
                    }
                    updateHiddenOtp();
                });
                input.addEventListener('keydown', function(e) {
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

        forgotPasswordOtpForm.addEventListener("submit", function (e) {
            e.preventDefault();
            updateHiddenOtp();
            
            forgotOtpError.style.display = "none";
            forgotOtpError.innerText = "";

            const formData = new FormData(forgotPasswordOtpForm);
            const submitBtn = forgotPasswordOtpForm.querySelector("button[type='submit']");
            submitBtn.disabled = true;
            submitBtn.innerText = "Verifying...";

            fetch("/Account/VerifyForgotPasswordOtp", {
                method: "POST",
                body: formData
            })
            .then(res => {
                if (res.redirected) {
                    // Success - redirect to reset password page
                    // For modal flow, we'll show the reset password modal instead
                    const otpModalInstance = bootstrap.Modal.getInstance(forgotPasswordOtpModal);
                    if (otpModalInstance) otpModalInstance.hide();

                    // Fill reset password modal with email
                    resetPasswordEmailInput.value = forgotOtpEmailInput.value;

                    // Show Reset Password Modal
                    const resetModalInstance = new bootstrap.Modal(resetPasswordModal);
                    resetModalInstance.show();
                    return;
                }
                return res.json();
            })
            .then(data => {
                if (data && data.success !== undefined) {
                    // JSON response (error case)
                    if (data.success) {
                        // Close OTP modal and show reset password modal
                        const otpModalInstance = bootstrap.Modal.getInstance(forgotPasswordOtpModal);
                        if (otpModalInstance) otpModalInstance.hide();

                        // Fill reset password modal with email
                        resetPasswordEmailInput.value = forgotOtpEmailInput.value;

                        // Show Reset Password Modal
                        const resetModalInstance = new bootstrap.Modal(resetPasswordModal);
                        resetModalInstance.show();
                    } else {
                        forgotOtpError.innerText = data.message || "Invalid OTP. Please try again.";
                        forgotOtpError.style.display = "block";
                    }
                }
            })
            .catch(err => {
                console.error(err);
                forgotOtpError.innerText = "An error occurred.";
                forgotOtpError.style.display = "block";
            })
            .finally(() => {
                submitBtn.disabled = false;
                submitBtn.innerText = "Continue";
            });
        });
    }

    // Resend Forgot Password OTP functionality
    if (resendForgotOtpLink) {
        resendForgotOtpLink.addEventListener('click', function(e) {
            e.preventDefault();
            const email = forgotOtpEmailInput.value;
            
            fetch('/Account/ResendForgotPasswordOtp', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ email: email })
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

    // Reset Password Form handling
    const resetPasswordForm = document.getElementById("resetPasswordForm");
    const resetPasswordError = document.getElementById("resetPasswordError");

    if (resetPasswordForm) {
        resetPasswordForm.addEventListener("submit", function (e) {
            e.preventDefault();
            
            resetPasswordError.style.display = "none";
            resetPasswordError.innerText = "";

            const formData = new FormData(resetPasswordForm);
            const submitBtn = resetPasswordForm.querySelector("button[type='submit']");
            submitBtn.disabled = true;
            submitBtn.innerText = "Resetting...";

            fetch("/Account/ResetPassword", {
                method: "POST",
                body: formData
            })
            .then(res => {
                if (res.redirected) {
                    // Success - show All Done modal instead of redirecting
                    const resetModalInstance = bootstrap.Modal.getInstance(resetPasswordModal);
                    if (resetModalInstance) resetModalInstance.hide();
                    
                    // Show All Done modal
                    showAllDoneModal("Password reset successful! You can now sign in with your new password.");
                    return;
                }
                return res.json();
            })
            .then(data => {
                if (data && data.success !== undefined) {
                    // JSON response (error case)
                    if (data.success) {
                        // Close reset password modal and show All Done modal
                        const resetModalInstance = bootstrap.Modal.getInstance(resetPasswordModal);
                        if (resetModalInstance) resetModalInstance.hide();
                        
                        // Show All Done modal
                        showAllDoneModal("Password reset successful! You can now sign in with your new password.");
                    } else {
                        resetPasswordError.innerText = data.message || "Failed to reset password. Please try again.";
                        resetPasswordError.style.display = "block";
                    }
                }
            })
            .catch(err => {
                console.error(err);
                resetPasswordError.innerText = "An error occurred.";
                resetPasswordError.style.display = "block";
            })
            .finally(() => {
                submitBtn.disabled = false;
                submitBtn.innerText = "Reset Password";
            });
        });
    }
}); 