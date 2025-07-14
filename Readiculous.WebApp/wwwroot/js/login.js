document.addEventListener("DOMContentLoaded", function () {
    const loginForm = document.getElementById("loginForm");
    const loginError = document.getElementById("loginError");

    // Only add the event listener if the form exists on the page
    if (loginForm) {
        loginForm.addEventListener("submit", function (e) {
            e.preventDefault();

            if (loginError) {
                loginError.style.display = "none";
            }

            const formData = new FormData(loginForm);
            const submitBtn = loginForm.querySelector("button[type='submit']");
            submitBtn.disabled = true;
            submitBtn.innerText = "Signing in...";

            fetch("/Account/LoginAjax", {
                method: "POST",
                body: formData,
                credentials: "same-origin"
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        // ✅ Reload or redirect to home after login
                        window.location.href = data.redirectUrl || "/";
                    } else {
                        if (loginError) {
                            loginError.innerText = data.message || "Login failed.";
                            loginError.style.display = "block";
                        }
                    }
                })
                .catch(err => {
                    console.error(err);
                    if (loginError) {
                        loginError.innerText = "Something went wrong.";
                        loginError.style.display = "block";
                    }
                })
                .finally(() => {
                    submitBtn.disabled = false;
                    submitBtn.innerText = "Sign In";
                });
        });
    }
});
