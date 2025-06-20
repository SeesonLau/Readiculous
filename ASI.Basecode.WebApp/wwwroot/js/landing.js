document.addEventListener("DOMContentLoaded", () => {
    const authModal = document.getElementById("authModal");

    // All modal sections
    const loginForm = document.getElementById("loginForm");
    const registerForm = document.getElementById("registerForm");
    const forgotForm = document.getElementById("forgotForm");
    const otpForm = document.getElementById("otpForm");
    const newPasswordForm = document.getElementById("newPasswordForm");
    const successForm = document.getElementById("successForm");

    // Navigation buttons
    const showRegister = document.getElementById("showRegister");
    const showLogin = document.getElementById("showLogin");
    const showForgot = document.getElementById("showForgot");

    const backToLogin = document.getElementById("backToLogin");
    const backToForgot = document.getElementById("backToForgot");
    const backToOTP = document.getElementById("backToOTP");

    // Open modal with Login form
    window.toggleAuthModal = () => {
        authModal.classList.remove("hidden");
        setTimeout(() => authModal.classList.add("show"), 10);
        showOnlyById("loginForm");
    };

    // Close modal function (called by X button and click-outside)
    window.closeAuthModal = () => {
        authModal.classList.remove("show");
        setTimeout(() => {
            authModal.classList.add("hidden");
        }, 200);
    };

    // Reusable function to show only the target form
    window.showOnlyById = function (id) {
        const formIds = [
            "loginForm",
            "registerForm",
            "forgotForm",
            "otpForm",
            "newPasswordForm",
            "successForm"
        ];
        formIds.forEach(formId => {
            const form = document.getElementById(formId);
            if (form) {
                form.classList.add("hidden");
            }
        });
        const targetForm = document.getElementById(id);
        if (targetForm) {
            targetForm.classList.remove("hidden");
        }
    };

    // Navigation Events
    showRegister?.addEventListener("click", (e) => {
        e.preventDefault();
        showOnlyById("registerForm");
    });

    showLogin?.addEventListener("click", (e) => {
        e.preventDefault();
        showOnlyById("loginForm");
    });

    showForgot?.addEventListener("click", (e) => {
        e.preventDefault();
        showOnlyById("forgotForm");
    });

    backToLogin?.addEventListener("click", (e) => {
        e.preventDefault();
        showOnlyById("loginForm");
    });

    backToForgot?.addEventListener("click", (e) => {
        e.preventDefault();
        showOnlyById("forgotForm");
    });

    backToOTP?.addEventListener("click", (e) => {
        e.preventDefault();
        showOnlyById("otpForm");
    });

    // GENRE PANEL TOGGLE
    window.toggleGenrePanel = function () {
        document.getElementById("genre-panel")?.classList.toggle("hidden");
    };

    // FILTER BUTTON SELECTION
    window.selectedFilter = 'TOP';

    window.selectFilter = function (type) {
        selectedFilter = type;

        document.getElementById('topBtn')?.classList.remove('active');
        document.getElementById('newBtn')?.classList.remove('active');

        if (type === 'TOP') {
            document.getElementById('topBtn')?.classList.add('active');
        } else {
            document.getElementById('newBtn')?.classList.add('active');
        }
    };

    // GENRE SELECTION REDIRECT
    window.updateBooks = function () {
        const genre = document.getElementById('genreSelect')?.value;

        if (genre === '') return;

        // Redirect to new URL:
        window.location.href = `/User/GenreBooks?filter=${selectedFilter}&genre=${genre}`;
    };
});

// Expose modal control functions globally
window.showOTPForm = function () {
    showOnlyById("otpForm");
};

window.showNewPasswordForm = function () {
    showOnlyById("newPasswordForm");
};

window.showSuccessForm = function () {
    showOnlyById("successForm");
};

window.showLoginForm = function () {
    showOnlyById("loginForm");
};

window.showForgotForm = function () {
    showOnlyById("forgotForm");
};

window.showResetPasswordForm = function () {
    showOnlyById("forgotForm");
};

window.showBackToOTP = function () {
    showOnlyById("otpForm");
};

// Placeholder sign-in/register logic
window.signIn = function () {
    alert("Sign-in logic goes here");
};

window.register = function () {
    alert("Registration logic goes here");
};
