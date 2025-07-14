document.addEventListener("DOMContentLoaded", function () {
    const registerForm = document.getElementById("registerForm");
    const errorBox = document.getElementById("registerError");
    const registerModal = document.getElementById("registerModal");

    if (!registerForm) return;

    function resetRegisterForm() {
        registerForm.reset();
        errorBox.innerText = "";
        errorBox.style.display = "none";
    }

    function showLoginModal() {
        const loginModal = document.getElementById("loginModal");
        if (loginModal) {
            const modal = new bootstrap.Modal(loginModal);
            modal.show();
        }
    }

    registerForm.addEventListener("submit", function (e) {
        e.preventDefault();

        const formData = new FormData(registerForm);
        const submitBtn = registerForm.querySelector("button[type='submit']");

        errorBox.innerText = "";
        errorBox.style.display = "none";

        submitBtn.disabled = true;
        submitBtn.innerText = "Signing up...";

        fetch("/Account/RegisterAjax", {
            method: "POST",
            body: formData,
            credentials: "same-origin"
        })
            .then(async res => {
                const text = await res.text();
                console.log("Server response:", text);

                let data;
                try {
                    data = JSON.parse(text);
                } catch (e) {
                    throw new Error("Response was not JSON. Check the Network tab.");
                }

                if (data.success) {
                    const modal = bootstrap.Modal.getInstance(registerModal);
                    if (modal) modal.hide();

                    resetRegisterForm();

                    setTimeout(() => {
                        showLoginModal();
                        alert("Registration successful! You can now log in.");
                    }, 300);
                } else {
                    errorBox.innerText = data.message || "Registration failed.";
                    errorBox.style.display = "block";
                }
            })
            .catch(err => {
                console.error(err);
                errorBox.innerText = err.message || "Something went wrong.";
                errorBox.style.display = "block";
            })
            .finally(() => {
                submitBtn.disabled = false;
                submitBtn.innerText = "Sign Up";
            });
    });

    // Expose reset for external access
    window.resetRegisterForm = resetRegisterForm;
});
