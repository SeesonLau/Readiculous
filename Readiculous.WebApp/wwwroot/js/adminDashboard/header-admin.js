document.addEventListener('DOMContentLoaded', function () {
    // Shared elements and functions
    const slider = document.querySelector('.header-nav-slider');
    const headerNavContainer = document.querySelector('.header-nav-container');
    const navButtons = document.querySelectorAll('.nav-btn');
    const userProfile = document.querySelector('.user-profile');
    const logoutBtn = document.getElementById('logout-btn');
    const dropdownLogoutBtn = document.getElementById('dropdown-logout-btn');

    // Debounce function for resize events 
    function debounce(func, wait) {
        let timeout;
        return function () {
            const context = this, args = arguments;
            clearTimeout(timeout);
            timeout = setTimeout(() => {
                func.apply(context, args);
            }, wait);
        };
    }

    // Slider position functionality
    function updateSliderPosition(button) {
        try {
            const activeButton = button || document.querySelector('.nav-btn.active');
            if (!activeButton || !slider || !headerNavContainer) return;

            const buttonRect = activeButton.getBoundingClientRect();
            const containerRect = headerNavContainer.getBoundingClientRect();

            slider.style.width = `${buttonRect.width}px`;
            slider.style.left = `${buttonRect.left - containerRect.left}px`;
        } catch (error) {
            console.error('Slider update error:', error);
        }
    }

    // Initialize slider with delay
    setTimeout(() => updateSliderPosition(), 50);

    // Navigation button click handlers
    navButtons.forEach(button => {
        button.addEventListener('click', function () {
            setTimeout(() => updateSliderPosition(this), 10);
        });
    });

    // Resize and mutation observers for slider
    window.addEventListener('resize', debounce(updateSliderPosition, 100));

    if (headerNavContainer) {
        new MutationObserver(mutations => {
            mutations.forEach(mutation => {
                if (mutation.addedNodes.length) updateSliderPosition();
            });
        }).observe(headerNavContainer, { childList: true, subtree: true });
    }

    // User profile dropdown functionality
    if (userProfile) {
        userProfile.addEventListener('click', function (e) {
            if (!e.target.closest('.profile-dropdown a')) {
                this.classList.toggle('show-dropdown');
            }
        });
    }

    // Close dropdown when clicking outside
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.user-profile')) {
            const openDropdown = document.querySelector('.user-profile.show-dropdown');
            if (openDropdown) openDropdown.classList.remove('show-dropdown');
        }
    });

    // Logout functionality
    if (logoutBtn && dropdownLogoutBtn) {
        const logoutUrl = logoutBtn.getAttribute('href');
        const { showModal } = setupLogoutConfirmation(logoutUrl);

        [logoutBtn, dropdownLogoutBtn].forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                showModal();
            });
        });
    }


    // Responsive navigation items in dropdown
    function moveNavItemsToDropdown() {
        if (window.innerWidth > 576) {
            // Clear dropdown items when above mobile breakpoint
            const navContainer = document.querySelector('.dropdown-nav-items-container');
            if (navContainer) navContainer.innerHTML = '';
            return;
        }

        const navContainer = document.querySelector('.dropdown-nav-items-container');
        const navButtons = document.querySelectorAll('.nav-btn:not(.dropdown-nav-item)'); 

        if (!navContainer || navButtons.length === 0) return;

        // Clear existing items first
        navContainer.innerHTML = '';

        // Add each nav button to dropdown
        navButtons.forEach(button => {
            const clone = button.cloneNode(true);
            clone.classList.add('dropdown-nav-item');
            if (button.classList.contains('active')) {
                clone.classList.add('active');
            }

            const icon = document.createElement('i');
            icon.className = getIconClass(button.textContent.trim());
            clone.insertBefore(icon, clone.firstChild);
            navContainer.appendChild(clone);
        });
    }

    function getIconClass(text) {
        switch (text) {
            case 'Home': return 'fas fa-home';
            case 'Users': return 'fas fa-users';
            case 'Books': return 'fas fa-book';
            case 'Genres': return 'fas fa-tags';
            default: return 'fas fa-circle';
        }
    }

    // Initialize responsive dropdown
    moveNavItemsToDropdown();
    window.addEventListener('resize', debounce(function () {
        moveNavItemsToDropdown(); updateSliderPosition();
    }, 200));
});
