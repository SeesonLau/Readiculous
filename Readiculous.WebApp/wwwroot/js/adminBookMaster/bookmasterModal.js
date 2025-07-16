document.getElementById('CoverImage').addEventListener('change', function (event) {
    const previewContainer = document.querySelector('.image-preview-container');
    const preview = document.getElementById('imagePreview');
    const placeholder = document.querySelector('.cover-image-placeholder');

    if (this.files && this.files[0]) {
        const reader = new FileReader();

        reader.onload = function (e) {
            preview.src = e.target.result;
            preview.classList.remove('d-none');
            placeholder.classList.add('d-none');
        }

        reader.readAsDataURL(this.files[0]);
    } else {
        preview.classList.add('d-none');
        placeholder.classList.remove('d-none');
    }
});
