document.addEventListener("DOMContentLoaded", function () {
    const loginForm = document.getElementById("loginForm");
    const errorBox = document.getElementById("loginError");

    if (!loginForm || !errorBox) return;

    // Reset form and error message
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
                if (!response.ok) throw new Error(`HTTP error: ${response.status}`);
                return response.json();
            })
            .then(data => {
                console.log("Login response:", data);

                if (data.success) {
                    // Hide modal
                    const loginModalEl = document.getElementById("loginModal");
                    const modalInstance = bootstrap.Modal.getInstance(loginModalEl);
                    if (modalInstance) modalInstance.hide();

                    // Optional fade-out effect
                    document.body.classList.add("fade-out");

                    // Redirect or reload after a short delay
                    setTimeout(() => {
                        if (data.redirectUrl) {
                            window.location.href = data.redirectUrl;
                        } else {
                            location.reload();
                        }
                    }, 300);
                } else {
                    errorBox.innerText = data.message || "Invalid login credentials.";
                    errorBox.style.display = "block";
                }
            })
            .catch(err => {
                console.error("Login error:", err);
                errorBox.innerText = err.message || "Something went wrong. Please try again.";
                errorBox.style.display = "block";
            })
            .finally(() => {
                submitBtn.disabled = false;
                submitBtn.innerText = "Sign In";
            });
    });

    window.resetLoginForm = resetLoginForm;
});
