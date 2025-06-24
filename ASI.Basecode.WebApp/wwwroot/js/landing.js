document.addEventListener("DOMContentLoaded", () => {
    const authModal = document.querySelector(".custom-modal");

    // Modal Sections
    const loginForm = document.getElementById("loginForm");
    const registerForm = document.getElementById("registerForm");
    const forgotForm = document.getElementById("forgotForm");
    const otpForm = document.getElementById("otpForm");
    const newPasswordForm = document.getElementById("newPasswordForm");
    const resetSuccessForm = document.getElementById("successForm");
    const registrationSuccessForm = document.getElementById("registrationSuccessForm");
    const loginRequiredBox = document.getElementById("loginRequiredBox");

    // Navigation buttons
    const showRegister = document.getElementById("showRegister");
    const showLogin = document.getElementById("showLogin");
    const showForgot = document.getElementById("showForgot");
    const showotpForm = document.getElementById("showotpForm");
    const shownewPass = document.getElementById("newPasswordForm");
    const backToLogin = document.getElementById("backToLogin");
    const backToForgot = document.getElementById("backToForgot");
    const backToOTP = document.getElementById("backToOTP");

    // Show modal
    window.toggleAuthModal = () => {
        if (!authModal) return;
        authModal.classList.remove("hidden");
        setTimeout(() => authModal.classList.add("show"), 10);
        showOnlyById("loginForm");
    };

    // Hide modal
    window.closeAuthModal = () => {
        if (!authModal) return;
        authModal.classList.remove("show");
        setTimeout(() => authModal.classList.add("hidden"), 200);
    };

    // Show only one section
    window.showOnlyById = function (id) {
        const formIds = [
            "loginForm",
            "registerForm",
            "forgotForm",
            "otpForm",
            "newPasswordForm",
            "successForm",
            "registrationSuccessForm",
            "loginRequiredBox"
        ];
        formIds.forEach(formId => {
            const form = document.getElementById(formId);
            if (form) form.classList.add("hidden");
        });
        const targetForm = document.getElementById(id);
        if (targetForm) targetForm.classList.remove("hidden");
    };  

    // Login required
    window.showLoginRequiredModal = () => {
        if (!authModal) return;
        authModal.classList.remove("hidden");
        setTimeout(() => authModal.classList.add("show"), 10);
        showOnlyById("loginRequiredBox");
    };

    // Navigation
    showRegister?.addEventListener("click", e => {
        e.preventDefault();
        showOnlyById("registerForm");
    });

    showLogin?.addEventListener("click", e => {
        e.preventDefault();
        showOnlyById("loginForm");
    });

    showForgot?.addEventListener("click", e => {
        e.preventDefault();
        showOnlyById("forgotForm");
    });

  
    backToLogin?.addEventListener("click", e => {
        e.preventDefault();
        showOnlyById("loginForm");
    });

    backToForgot?.addEventListener("click", e => {
        e.preventDefault();
        showOnlyById("forgotForm");
    });

    backToOTP?.addEventListener("click", e => {
        e.preventDefault();
        showOnlyById("otpForm");
    });

    // Admin Sign-In
    window.signIn = () => {
        const username = document.getElementById("login-username")?.value.trim();
        const password = document.getElementById("login-password")?.value.trim();
        const errorBox = document.getElementById("loginError");

        if (username === "admin" && password === "admin123") {
            window.location.href = "/BookMaster/ListBooks";
        } else {
            if (errorBox) {
                errorBox.innerText = "Invalid credentials.";
                errorBox.style.display = "block";
            } else {
                alert("Invalid credentials.");
            }
        }
        return false;
    };

    // Sign-Up Logic
    window.register = () => {
        const username = document.getElementById("register-username")?.value.trim() || "[YourUsername]";
        const email = document.getElementById("register-email")?.value.trim() || "[YourEmail]";

        // Fill placeholders
        const uname = document.getElementById("success-username");
        const uemail = document.getElementById("success-email");

        if (uname) uname.innerText = username;
        if (uemail) uemail.innerText = email;

        showOnlyById("registrationSuccessForm");
    };

    window.showOTPForm = function () {
        showOnlyById("otpForm");
    };

    window.showNewPasswordForm = function () {
        showOnlyById("newPasswordForm");
    };

    window.showSuccessForm = function () {
        showOnlyById("successForm");
    };

    // Genre dropdown logic (if used)
    const genreDropdown = document.getElementById("genreDropdown");
    const genreList = document.getElementById("genreList");
    const selectedGenresDisplay = document.getElementById("selectedGenres");
    const selectedGenresInput = document.getElementById("selectedGenresInput");

    let selectedGenres = [];

    document.getElementById("genreDropdownBtn")?.addEventListener("click", e => {
        e.stopPropagation();
        genreList.classList.toggle("show");
    });

    genreList?.querySelectorAll("li").forEach(item => {
        item.addEventListener("click", e => {
            e.stopPropagation();
            const genre = item.getAttribute("data-value");

            if (selectedGenres.includes(genre)) {
                selectedGenres = selectedGenres.filter(g => g !== genre);
                item.classList.remove("selected");
            } else {
                if (selectedGenres.length < 5) {
                    selectedGenres.push(genre);
                    item.classList.add("selected");
                } else {
                    alert("You can only select up to 5 genres.");
                }
            }

            selectedGenresInput.value = selectedGenres.join(',');
            renderSelectedGenres();
        });
    });

    function renderSelectedGenres() {
        selectedGenresDisplay.innerHTML = "";
        if (selectedGenres.length === 0) {
            selectedGenresDisplay.innerHTML = `<p class="text-muted">No genres selected.</p>`;
            return;
        }

        selectedGenres.forEach(genre => {
            const span = document.createElement("span");
            span.classList.add("badge", "bg-primary", "me-1");
            span.innerText = genre;
            selectedGenresDisplay.appendChild(span);
        });
    }

    document.addEventListener("click", e => {
        if (!genreDropdown?.contains(e.target)) {
            genreList?.classList.remove("show");
        }
    });
});
