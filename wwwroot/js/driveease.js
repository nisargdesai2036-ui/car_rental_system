// DriveEase Interactive UI Scripts

document.addEventListener('DOMContentLoaded', () => {
    // 1. Mobile Navigation Drawer Toggle
    const mobileToggleBtn = document.getElementById('mobileNavToggle');
    const mobileDrawer = document.getElementById('mobileDrawer');
    
    if (mobileToggleBtn && mobileDrawer) {
        mobileToggleBtn.addEventListener('click', () => {
            const isOpen = mobileDrawer.classList.toggle('open');
            const icon = mobileToggleBtn.querySelector('.material-symbols-outlined');
            if (icon) {
                icon.textContent = isOpen ? 'close' : 'menu';
            }
        });
    }

    // 2. User Profile Dropdown Toggle
    const profileDropdownTrigger = document.getElementById('userProfileDropdownTrigger');
    const profileDropdownMenu = document.getElementById('userProfileDropdownMenu');

    if (profileDropdownTrigger && profileDropdownMenu) {
        profileDropdownTrigger.addEventListener('click', (e) => {
            e.stopPropagation();
            profileDropdownMenu.classList.toggle('show');
        });

        document.addEventListener('click', (e) => {
            if (!profileDropdownMenu.contains(e.target) && !profileDropdownTrigger.contains(e.target)) {
                profileDropdownMenu.classList.remove('show');
            }
        });
    }

    // 3. Global Keyboard Shortcut (⌘K / Ctrl+K) for Quick Search
    document.addEventListener('keydown', (e) => {
        if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 'k') {
            e.preventDefault();
            const searchInput = document.getElementById('globalSearchInput') || document.querySelector('.search-quick-btn');
            if (searchInput) {
                searchInput.click();
            }
        }
    });
});
