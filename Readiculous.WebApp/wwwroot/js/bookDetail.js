function showFavoriteMessage() {
    const alertBox = document.getElementById("favoriteAlert");
    alertBox.classList.remove("d-none");
    setTimeout(() => alertBox.classList.add("d-none"), 3000);
}

function hideFavoriteMessage() {
    document.getElementById("favoriteAlert").classList.add("d-none");
}

function setRating(value) {
    $("#Rating").val(value);
    $("#ratingWidget i").each(function () {
        const starValue = $(this).data("value");
        if (starValue <= value) {
            $(this).removeClass("bi-star").addClass("bi-star-fill text-warning").removeClass("text-secondary");
        } else {
            $(this).removeClass("bi-star-fill text-warning").addClass("bi-star text-secondary");
        }
    });
}

$(document).ready(function () {
    $(document).on("submit", "#addReviewForm", function (e) {
        e.preventDefault();

        const formData = {
            ReviewId: "",
            BookId: $("#BookId").val(),
            UserId: $("#UserId").val(),
            Rating: parseInt($("#Rating").val()),
            Comment: $("#Comment").val(),
            CreatedTime: new Date().toISOString(),
            UserName: $("#UserName").val(),
            Email: $("#Email").val()
        };

        $.ajax({
            url: "/BookDetail/CreateReview",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(formData),
            success: function (response) {
                $("#addReviewModal").modal("hide");
                $("#no-reviews-text").remove();

                let starsHtml = "";
                for (let i = 1; i <= 5; i++) {
                    starsHtml += i <= response.rating
                        ? '<i class="bi bi-star-fill text-warning"></i>'
                        : '<i class="bi bi-star text-muted"></i>';
                }

                const html = `
                    <div class="card mb-3 shadow-sm">
                        <div class="card-body d-flex align-items-start gap-3">
                            <img src="/img/default-profile.png" class="rounded-circle" style="width:40px; height:40px;" />
                            <div>
                                <h6 class="mb-1">${response.reviewer}</h6>
                                <div class="mb-1">
                                    ${starsHtml}
                                    <span class="text-muted small">(${response.rating.toFixed(1)})</span>
                                </div>
                                <p class="mb-1">${response.comment}</p>
                                <small class="text-muted">${response.createdTime}</small>
                            </div>
                        </div>
                    </div>`;

                $("#reviews-container").prepend(html);

                const count = parseInt($("#review-count").text()) + 1;
                $("#review-count").text(count);
            },
            error: function (xhr) {
                alert("Error: " + xhr.responseText);
            }
        });
    });
});
