document.getElementById('CoverImage').addEventListener('change', function (event) {
    const previewContainer = document.getElementById('imagePreviewContainer');
    const preview = document.getElementById('imagePreview');

    if (this.files && this.files[0]) {
        const reader = new FileReader();

        reader.onload = function (e) {
            preview.src = e.target.result;
            previewContainer.classList.remove('d-none');
        }

        reader.readAsDataURL(this.files[0]);
    } else {
        previewContainer.classList.add('d-none');
    }
});
