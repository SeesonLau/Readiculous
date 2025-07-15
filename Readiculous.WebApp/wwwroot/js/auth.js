document.addEventListener('DOMContentLoaded', function () {
    const container = document.getElementById('auth-container');
    if (!container) return;

    // Helper to load a partial via AJAX
    function loadPartial(url, data = null, method = 'GET') {
        const options = { method };
        if (data) {
            options.headers = { 'Content-Type': 'application/json' };
            options.body = JSON.stringify(data);
        }
        fetch(url, options)
            .then(res => res.text())
            .then(html => {
                container.innerHTML = html;
                bindEvents();
            });
    }

    // Initial load: login
    loadPartial('/Account/AuthPartial?view=login');

    function bindEvents() {
        // Login
        const loginForm = document.getElementById('loginForm');
        if (loginForm) {
            loginForm.addEventListener('submit', function (e) {
                e.preventDefault();
                const formData = new FormData(loginForm);
                fetch('/Account/Login', {
                    method: 'POST',
                    body: formData,
                    credentials: 'same-origin',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                })
                    .then(res => res.json())
                    .then(data => {
                        if (data.success && data.firstTime) {
                            // Show first time profile modal/partial
                            loadPartial('/Account/FirstTimeProfile');
                        } else if (data.success && data.redirectUrl) {
                            window.location.href = data.redirectUrl;
                        } else {
                            // Show specific error messages from backend
                            toastr.error(data.message || 'Wrong Email or Password');
                        }
                    });
            });
            document.getElementById('show-forgot')?.addEventListener('click', function (e) {
                e.preventDefault();
                loadPartial('/Account/AuthPartial?view=forgot');
            });
            document.getElementById('show-signup')?.addEventListener('click', function (e) {
                e.preventDefault();
                loadPartial('/Account/AuthPartial?view=register');
            });
        }
        // Register
        const registerForm = document.getElementById('registerForm');
        if (registerForm) {
            registerForm.addEventListener('submit', function (e) {
                e.preventDefault();
                const formData = new FormData(registerForm);
                fetch('/Account/RequestSignupOtp', {
                    method: 'POST',
                    body: formData,
                    credentials: 'same-origin',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                })
                    .then(res => res.json())
                    .then(data => {
                        if (data.success) {
                            // Load the OTP partial with the email (signup flow)
                            loadPartial('/Account/AuthPartial?view=otp&email=' + encodeURIComponent(formData.get('Email')) + '&flow=signup');
                            toastr.success('OTP has been sent to your email.');
                        } else {
                            toastr.error(data.message || 'Email already exists.');
                        }
                    });
            });
            document.getElementById('show-login')?.addEventListener('click', function (e) {
                e.preventDefault();
                loadPartial('/Account/AuthPartial?view=login');
            });
        }
        // Forgot Password
        const forgotForm = document.getElementById('forgotForm');
        if (forgotForm) {
            forgotForm.addEventListener('submit', function (e) {
                e.preventDefault();
                const formData = new FormData(forgotForm);
                fetch('/Account/RequestForgotPasswordOtp', {
                    method: 'POST',
                    body: formData,
                    credentials: 'same-origin',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                })
                    .then(res => res.json())
                    .then(data => {
                        if (data.success) {
                            // Load the OTP partial with the email (forgot flow)
                            loadPartial('/Account/AuthPartial?view=otp&email=' + encodeURIComponent(formData.get('Email')) + '&flow=forgot');
                            toastr.success('OTP has been sent to your email.');
                        } else {
                            toastr.error(data.message || 'Email does not exist.');
                        }
                    });
            });
            document.getElementById('show-login')?.addEventListener('click', function (e) {
                e.preventDefault();
                loadPartial('/Account/AuthPartial?view=login');
            });
        }
        // OTP
        const otpForm = document.getElementById('otpForm');
        if (otpForm) {
            const flow = otpForm.getAttribute('data-flow') || 'signup';
            const inputs = otpForm.querySelectorAll('.otp-input');
            const hiddenOtp = document.getElementById('OtpHidden');
            if (inputs.length && hiddenOtp) {
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
                function updateHiddenOtp() {
                    let otp = '';
                    inputs.forEach(i => otp += i.value);
                    hiddenOtp.value = otp;
                }
                otpForm.addEventListener('submit', function (e) {
                    e.preventDefault();
                    updateHiddenOtp();
                    const formData = new FormData(otpForm);
                    const email = formData.get('Email');
                    let url, onSuccess;
                    if (flow === 'forgot') {
                        url = '/Account/VerifyForgotPasswordOtp';
                        onSuccess = () => loadPartial('/Account/AuthPartial?view=reset&email=' + encodeURIComponent(email));
                    } else {
                        url = '/Account/VerifySignupOtp';
                        onSuccess = () => loadPartial('/Account/AuthPartial?view=success');
                    }
                    fetch(url, {
                        method: 'POST',
                        body: formData,
                        credentials: 'same-origin',
                        headers: { 'X-Requested-With': 'XMLHttpRequest' }
                    })
                        .then(res => res.json())
                        .then(data => {
                            if (data.success) {
                                onSuccess();
                                toastr.success('OTP verified!');
                            } else {
                                toastr.error(data.message || 'Invalid OTP. Please try again.');
                            }
                        });
                });
            }
            document.getElementById('show-login')?.addEventListener('click', function (e) {
                e.preventDefault();
                loadPartial('/Account/AuthPartial?view=login');
            });
        }
        // Reset Password
        const resetPasswordForm = document.getElementById('resetPasswordForm');
        if (resetPasswordForm) {
            resetPasswordForm.addEventListener('submit', function (e) {
                e.preventDefault();
                const formData = new FormData(resetPasswordForm);
                fetch('/Account/ResetPassword', {
                    method: 'POST',
                    body: formData,
                    credentials: 'same-origin',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                })
                    .then(res => res.json())
                    .then(data => {
                        if (data.success) {
                            // Show the success partial with the reset password message
                            loadPartial('/Account/AuthPartial?view=success');
                            toastr.success('Your password has been reset.');
                        } else {
                            toastr.error(data.message || 'Failed to update password. Please try again.');
                        }
                    });
            });
        }
        // Success
        const successLoginBtn = document.getElementById('success-login-btn');
        if (successLoginBtn) {
            successLoginBtn.addEventListener('click', function () {
                loadPartial('/Account/AuthPartial?view=login');
            });
        }
        // First Time Profile
        const firstTimeProfileForm = document.getElementById('firstTimeProfileForm');
        if (firstTimeProfileForm) {
            firstTimeProfileForm.addEventListener('submit', function (e) {
                e.preventDefault();
                const formData = new FormData(firstTimeProfileForm);
                fetch('/Account/CompleteFirstTimeProfile', {
                    method: 'POST',
                    body: formData,
                    credentials: 'same-origin',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                })
                    .then(res => res.json())
                    .then(data => {
                        if (data.success && data.redirectUrl) {
                            window.location.href = data.redirectUrl;
                        } else if (data.success) {
                            loadPartial('/Account/AuthPartial?view=success');
                        } else {
                            toastr.error('Failed to complete profile.');
                        }
                    })
                    .catch(() => toastr.error('Failed to complete profile.'));
            });
        }
    }
}); 