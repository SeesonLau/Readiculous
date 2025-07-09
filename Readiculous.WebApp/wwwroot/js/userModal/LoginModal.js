document.addEventListener("DOMContentLoaded", function () {
    const loginForm = document.getElementById("loginForm");
    const errorBox = document.getElementById("loginError");
    const loginModal = document.querySelector('.auth-box[data-id="login"]');

    if (!loginForm) return;

    // Reset fields when modal is shown
    function resetLoginForm() {
        loginForm.reset();
        errorBox.innerText = "";
        errorBox.style.display = "none";
    }

    loginForm.addEventListener("submit", function (e) {
        e.preventDefault();

        const formData = new FormData(loginForm);
        const submitBtn = loginForm.querySelector("button[type='submit']");

        errorBox.innerText = "";
        errorBox.style.display = "none";

        submitBtn.disabled = true;
        submitBtn.innerText = "Signing in...";

        fetch("/Account/LoginAjax", {
            method: "POST",
            body: formData,
            credentials: "same-origin"
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                console.log("Login response:", data);

                if (data.success) {
                    if (data.role === "Admin") {
                        window.location = "/Home/Index";
                    } else {
                        window.location = "/UserView/GuestView";
                    }
                } else {
                    errorBox.innerText = data.message || "Login failed.";
                    errorBox.style.display = "block";
                }
            })
            .catch(err => {
                console.error("Login error:", err);
                errorBox.innerText = err.message || "Something went wrong.";
                errorBox.style.display = "block";
            })
            .finally(() => {
                submitBtn.disabled = false;
                submitBtn.innerText = "Sign In";
            });
    });

    // Expose reset for switching modals
    window.resetLoginForm = resetLoginForm;
});
