document.addEventListener('DOMContentLoaded', function () {
    const container = document.getElementById('auth-container');
    if (!container) return;

    function toggleFormButtons(form, disable) {
        const buttons = form.querySelectorAll('button, a');
        buttons.forEach(btn => {
            if (btn.tagName === 'BUTTON' || btn.tagName === 'A') {
                btn.disabled = disable;
                btn.style.pointerEvents = disable ? 'none' : '';
            }
        });
    }

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

    loadPartial('/Account/AuthPartial?view=login');
    const submittingForms = new Set();

    function bindEvents() {
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
                            loadPartial('/Account/FirstTimeProfile');
                        } else if (data.success && data.redirectUrl) {
                            window.location.href = data.redirectUrl;
                        } else {
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

        const registerForm = document.getElementById('registerForm');
        function registerSubmitHandler(e) {
            if (submittingForms.has(registerForm)) return;
            e.preventDefault();
            submittingForms.add(registerForm);
            toggleFormButtons(registerForm, true);
            const submitBtn = registerForm.querySelector('button[type="submit"]');
            let originalText = '';
            if (submitBtn) {
                originalText = submitBtn.innerHTML;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Processing';
            }
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
                        loadPartial('/Account/AuthPartial?view=otp&email=' + encodeURIComponent(formData.get('Email')) + '&flow=signup');
                        toastr.success('OTP has been sent to your email.');
                    } else {
                        toastr.error(data.message || 'Email already exists.');
                        toggleFormButtons(registerForm, false);
                        if (submitBtn) submitBtn.innerHTML = originalText;
                    }
                })
                .catch(() => {
                    toggleFormButtons(registerForm, false);
                    if (submitBtn) submitBtn.innerHTML = originalText;
                })
                .finally(() => { submittingForms.delete(registerForm); });
        }
        if (registerForm) {
            registerForm.removeEventListener('submit', registerSubmitHandler);
            registerForm.addEventListener('submit', registerSubmitHandler);
            document.getElementById('show-login')?.addEventListener('click', function (e) {
                e.preventDefault();
                loadPartial('/Account/AuthPartial?view=login');
            });
        }

        const forgotForm = document.getElementById('forgotForm');
        function forgotSubmitHandler(e) {
            if (submittingForms.has(forgotForm)) return;
            e.preventDefault();
            submittingForms.add(forgotForm);
            toggleFormButtons(forgotForm, true);
            const submitBtn = forgotForm.querySelector('button[type="submit"]');
            let originalText = '';
            if (submitBtn) {
                originalText = submitBtn.innerHTML;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Processing';
            }
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
                        loadPartial('/Account/AuthPartial?view=otp&email=' + encodeURIComponent(formData.get('Email')) + '&flow=forgot');
                        toastr.success('OTP has been sent to your email.');
                    } else {
                        toastr.error(data.message || 'Email does not exist.');
                        toggleFormButtons(forgotForm, false);
                        if (submitBtn) submitBtn.innerHTML = originalText;
                    }
                })
                .catch(() => {
                    toggleFormButtons(forgotForm, false);
                    if (submitBtn) submitBtn.innerHTML = originalText;
                })
                .finally(() => { submittingForms.delete(forgotForm); });
        }
        if (forgotForm) {
            forgotForm.removeEventListener('submit', forgotSubmitHandler);
            forgotForm.addEventListener('submit', forgotSubmitHandler);
            document.getElementById('show-login')?.addEventListener('click', function (e) {
                e.preventDefault();
                loadPartial('/Account/AuthPartial?view=login');
            });
        }

        const otpForm = document.getElementById('otpForm');
        if (otpForm) {
            const flow = otpForm.getAttribute('data-flow') || 'signup';
            const inputs = otpForm.querySelectorAll('.otp-input');
            const hiddenOtp = document.getElementById('OtpHidden');
            if (inputs.length && hiddenOtp) {
                inputs[0].focus();
                inputs.forEach((input, idx) => {
                    input.addEventListener('input', function () {
                        if (this.value.length > 1) this.value = this.value.slice(0, 1);
                        if (this.value && idx < inputs.length - 1) inputs[idx + 1].focus();
                        updateHiddenOtp();
                    });
                    input.addEventListener('keydown', function (e) {
                        if (e.key === 'Backspace' && !this.value && idx > 0) inputs[idx - 1].focus();
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
                    toggleFormButtons(otpForm, true);
                    const submitBtn = otpForm.querySelector('button[type="submit"]');
                    let originalText = '';
                    if (submitBtn) {
                        originalText = submitBtn.innerHTML;
                        submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Verifying';
                    }
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
                                toggleFormButtons(otpForm, false);
                                if (submitBtn) submitBtn.innerHTML = originalText;
                            }
                        })
                        .catch(() => {
                            toastr.error('An error occurred.');
                            toggleFormButtons(otpForm, false);
                            if (submitBtn) submitBtn.innerHTML = originalText;
                        })
                        .finally(() => { submittingForms.delete(otpForm); });
                });
            }
            document.getElementById('show-login')?.addEventListener('click', function (e) {
                e.preventDefault();
                loadPartial('/Account/AuthPartial?view=login');
            });
        }


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
                            loadPartial('/Account/AuthPartial?view=success');
                            toastr.success('Your password has been reset.');
                        } else {
                            toastr.error(data.message || 'Failed to update password. Please try again.');
                        }
                    });
            });
        }

        const successLoginBtn = document.getElementById('success-login-btn');
        if (successLoginBtn) {
            successLoginBtn.addEventListener('click', function () {
                loadPartial('/Account/AuthPartial?view=login');
            });
        }

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
