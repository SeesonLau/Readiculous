document.addEventListener("DOMContentLoaded", function () {
    const registerForm = document.getElementById("registerForm");
    const errorBox = document.getElementById("registerError");
    const registerModal = document.querySelector('.auth-box[data-id="register"]');

    if (!registerForm) return;

    function resetRegisterForm() {
        registerForm.reset();
        errorBox.innerText = "";
        errorBox.style.display = "none";
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
            .then(res => res.text())
            .then(text => {
                console.log("Server response:", text);

                let data;
                try {
                    data = JSON.parse(text);
                } catch (e) {
                    throw new Error("Response was not JSON. Check the Network tab.");
                }

                if (data.success) {
                    // Close register modal
                    closeModal();

                    // Reset register form so it is clean next time
                    resetRegisterForm();

                    // Show login modal
                    showModal("login");

                    // Optionally reset login form as well
                    if (window.resetLoginForm) {
                        window.resetLoginForm();
                    }

                    setTimeout(() => {
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

    // Expose reset for switching modals
    window.resetRegisterForm = resetRegisterForm;
});
