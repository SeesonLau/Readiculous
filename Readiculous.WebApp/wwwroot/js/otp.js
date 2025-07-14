document.addEventListener("DOMContentLoaded", function () {
  const registerForm = document.getElementById("registerForm");
    const registerError = document.getElementById("registerError");
    const registerModal = document.getElementById("registerModal");

    const otpModal = document.getElementById("otpModal");
    const otpEmailInput = document.getElementById("otpEmail");
    const otpEmailDisplay = document.getElementById("otpEmailDisplay");

    registerForm.addEventListener("submit", function (e) {
        e.preventDefault();

    const formData = new FormData(registerForm);
    const submitBtn = registerForm.querySelector("button[type='submit']");
    registerError.innerText = "";
    registerError.style.display = "none";

    submitBtn.disabled = true;
    submitBtn.innerText = "Sending OTP...";

    fetch("/Account/RegisterAjax", {
        method: "POST",
    body: formData
    })
    .then(res => res.json())
    .then(data => {
      if (data.success) {
        // Hide Register modal
        const modalInstance = bootstrap.Modal.getInstance(registerModal);
    modalInstance.hide();

    // Show OTP modal
    otpEmailInput.value = formData.get("Email");
    otpEmailDisplay.innerText = formData.get("Email");

    const otpModalInstance = new bootstrap.Modal(otpModal);
    otpModalInstance.show();

      } else {
        registerError.innerText = data.message;
    registerError.style.display = "block";
      }
    })
    .catch(err => {
        registerError.innerText = "Something went wrong.";
    registerError.style.display = "block";
    })
    .finally(() => {
        submitBtn.disabled = false;
    submitBtn.innerText = "Sign Up";
    });
  });
});

