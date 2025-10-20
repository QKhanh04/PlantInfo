AOS.init({
    duration: 800,
    easing: 'ease-out-cubic',
    once: true
});



let thumbStart = 0;
const thumbPerPage = 5;

function renderThumbnails() {
    const container = document.getElementById('thumbnailList');
    container.innerHTML = '';
    for (let i = thumbStart; i < Math.min(imageList.length, thumbStart + thumbPerPage); i++) {
        const img = imageList[i];
        const el = document.createElement('img');
        el.src = img.url;
        el.className = 'thumbnail-img' + (img.isPrimary ? ' active' : '');
        el.onclick = function () {
            document.getElementById('mainPlantImage').src = img.url;
            document.querySelectorAll('.thumbnail-img').forEach(t => t.classList.remove('active'));
            el.classList.add('active');
        };
        container.appendChild(el);
    }
    document.getElementById('thumbPrev').disabled = (thumbStart === 0);
    document.getElementById('thumbNext').disabled = (thumbStart + thumbPerPage >= imageList.length);
}

function showThumbnail(direction) {
    thumbStart += direction;
    if (thumbStart < 0) thumbStart = 0;
    if (thumbStart > imageList.length - thumbPerPage) thumbStart = imageList.length - thumbPerPage;
    renderThumbnails();
}

// Image Modal
function openImageModal() {
    const modal = document.getElementById('imageModal');
    const modalImg = document.getElementById('modalImage');
    const mainImg = document.getElementById('mainPlantImage');
    modal.style.display = "block";
    modalImg.src = mainImg.src;
}

document.querySelector('.close-modal')?.addEventListener('click', function () {
    document.getElementById('imageModal').style.display = "none";
});

// Toggle Edit Mode
function toggleEditMode() {
    const viewMode = document.getElementById('reviewViewMode');
    const editMode = document.getElementById('reviewEditMode');

    if (viewMode.style.display === 'none') {
        viewMode.style.display = 'block';
        editMode.style.display = 'none';
    } else {
        viewMode.style.display = 'none';
        editMode.style.display = 'block';
        const currentRating = parseInt(document.getElementById('edit-rating-value').value);
        updateEditStars(currentRating);
    }
}

// Star rating for Add Review
document.addEventListener('DOMContentLoaded', function () {
    renderThumbnails();

    const addStarRating = document.getElementById('addStarRating');
    if (addStarRating) {
        initializeStarRating(addStarRating, 'add-rating-value');
    }

    const editStarRating = document.getElementById('editStarRating');
    if (editStarRating) {
        initializeStarRating(editStarRating, 'edit-rating-value');
        const currentRating = parseInt(document.getElementById('edit-rating-value').value);
        updateStarsDisplay(editStarRating, currentRating);
    }

    // Tooltip functionality
    const badgeItems = document.querySelectorAll('.badge-custom, .badge-item');
    let tooltipEl = null;
    let popperInstance = null;

    badgeItems.forEach(badge => {
        badge.addEventListener('mouseenter', function () {
            tooltipEl = document.createElement('div');
            tooltipEl.className = 'custom-tooltip';
            tooltipEl.innerHTML = badge.getAttribute('data-tooltip-content');
            document.body.appendChild(tooltipEl);

            popperInstance = Popper.createPopper(badge, tooltipEl, {
                placement: 'top',
                modifiers: [{ name: 'offset', options: { offset: [0, 8] } }]
            });
        });

        badge.addEventListener('mouseleave', function () {
            if (tooltipEl) {
                document.body.removeChild(tooltipEl);
                tooltipEl = null;
            }
            if (popperInstance) {
                popperInstance.destroy();
                popperInstance = null;
            }
        });
    });
});

function initializeStarRating(container, inputId) {
    const stars = container.querySelectorAll('i');
    let selectedRating = parseInt(document.getElementById(inputId).value) || 0;

    stars.forEach(star => {
        star.addEventListener('click', function () {
            selectedRating = parseInt(this.getAttribute('data-rating'));
            updateStarsDisplay(container, selectedRating);
            document.getElementById(inputId).value = selectedRating;
        });

        star.addEventListener('mouseenter', function () {
            const rating = parseInt(this.getAttribute('data-rating'));
            updateStarsDisplay(container, rating);
        });
    });

    container.addEventListener('mouseleave', function () {
        updateStarsDisplay(container, selectedRating);
    });

    if (selectedRating > 0) {
        updateStarsDisplay(container, selectedRating);
    }
}

function updateStarsDisplay(container, rating) {
    const stars = container.querySelectorAll('i');
    stars.forEach(star => {
        const starRating = parseInt(star.getAttribute('data-rating'));
        if (starRating <= rating) {
            star.classList.remove('bi-star');
            star.classList.add('bi-star-fill');
        } else {
            star.classList.remove('bi-star-fill');
            star.classList.add('bi-star');
        }
    });
}

function updateEditStars(rating) {
    const editStarRating = document.getElementById('editStarRating');
    if (editStarRating) {
        updateStarsDisplay(editStarRating, rating);
    }
}

$('#favoriteBtn').on('click', function (e) {
    //e.preventDefault();
    var plantId = $(this).data('plant-id');
    var token = $('input[name="__RequestVerificationToken"]').val();
    var btn = $(this);

    $.ajax({
        url: '/Detail/' + plantId + '?handler=ToggleFavorite',
        type: 'POST',
        data: { __RequestVerificationToken: token },
        success: function (response) {
            if (response.isFavorited) {
                btn.addClass('favorited');
                btn.find('i').removeClass('bi-heart').addClass('bi-heart-fill');
                btn.find('span').text('Đã yêu thích');
            } else {
                btn.removeClass('favorited');
                btn.find('i').removeClass('bi-heart-fill').addClass('bi-heart');
                btn.find('span').text('Yêu thích');
            }
        },
        error: function (xhr, status, error) {
            alert('Có lỗi xảy ra khi cập nhật yêu thích! ' + xhr.status + ' ' + xhr.responseText);
        }
    });
});



const editForm = document.getElementById('editReviewForm');
if (editForm) {
    document.getElementById('editReviewForm').addEventListener('submit', async function (e) {
        e.preventDefault();

        // Lấy dữ liệu từ form
        const rating = document.getElementById('edit-rating-value').value;
        console.log("Rating: ", rating);

        const comment = document.getElementById('edit-review-comment').value;
        console.log("comment: ", comment);

        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        console.log("token: ", token);

        // Tạo dữ liệu gửi lên server
        const formData = new URLSearchParams();
        formData.append('UpdateReview.Rating', rating);
        formData.append('UpdateReview.Comment', comment);
        formData.append('__RequestVerificationToken', token);

        try {
            const response = await fetch(window.location.pathname + '?handler=UpdateReview', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: formData.toString()

            });
            //console.log("body: ", body); 
            const data = await response.json();
            console.log("data: ", data);

            if (response.ok && data.success) {

                location.reload();

            } else {
                // Thông báo lỗi
                alert(data.message || "Cập nhật không thành công!");
            }
        } catch (err) {
            alert('Không thể gửi yêu cầu. Vui lòng thử lại!');
        }
    });

}


const addForm = document.getElementById('addReviewForm');
if (addForm) {
    document.getElementById('addReviewForm').addEventListener('submit', async function (e) {
        e.preventDefault();

        // Lấy dữ liệu từ form
        const rating = document.getElementById('add-rating-value').value;
        console.log("Rating: ", rating);

        const comment = document.getElementById('add-review-comment').value;
        console.log("comment: ", comment);

        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        console.log("token: ", token);

        // Tạo dữ liệu gửi lên server
        const formData = new URLSearchParams();
        formData.append('AddReview.Rating', rating);
        formData.append('AddReview.Comment', comment);
        formData.append('__RequestVerificationToken', token);

        try {
            const response = await fetch(window.location.pathname + '?handler=AddReview', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: formData.toString()
            });

            const data = await response.json();
            console.log(data)
            if (response.ok && data.success) {
                location.reload();
            } else {
                alert(data.message || "Thêm đánh giá không thành công!");
            }
        } catch (err) {
            alert('Không thể gửi yêu cầu. Vui lòng thử lại!');
        }
    });
}
