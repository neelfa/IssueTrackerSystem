// Enhanced IssueTracker JavaScript
// Modern interactions and animations (Simplified - Dark theme removed)
// Added sidebar toggle functionality

document.addEventListener('DOMContentLoaded', function() {
    initializeAnimations();
    initializeInteractions();
    initializeResponsiveUtils();
    initializeLoadingStates();
    initializeTooltips();
    initializeAccessibility();
    initializeSidebarToggle();
});

// Sidebar Toggle Functionality
function initializeSidebarToggle() {
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.getElementById('mainContent');
    const sidebarOverlay = document.getElementById('sidebarOverlay');
    
    if (!sidebarToggle || !sidebar || !mainContent) return;
    
    // Set initial state based on screen size
    function setInitialState() {
        const isLargeScreen = window.innerWidth > 992;
        const topHeader = document.querySelector('.top-header');
        
        if (isLargeScreen) {
            sidebar.classList.remove('collapsed');
            mainContent.classList.remove('expanded');
            if (sidebarOverlay) sidebarOverlay.classList.remove('show');
            if (topHeader) {
                topHeader.style.left = 'var(--sidebar-width)';
            }
        } else {
            sidebar.classList.add('collapsed');
            mainContent.classList.add('expanded');
            sidebar.classList.remove('show');
            if (topHeader) {
                topHeader.style.left = '0';
            }
        }
    }
    
    // Toggle sidebar function
    function toggleSidebar() {
        const isLargeScreen = window.innerWidth > 992;
        const topHeader = document.querySelector('.top-header');
        
        if (isLargeScreen) {
            // Desktop behavior - slide sidebar and adjust main content
            sidebar.classList.toggle('collapsed');
            mainContent.classList.toggle('expanded');
            
            // Update header positioning
            if (topHeader) {
                if (sidebar.classList.contains('collapsed')) {
                    topHeader.style.left = '0';
                } else {
                    topHeader.style.left = 'var(--sidebar-width)';
                }
            }
            
            // Update toggle button icon
            updateToggleIcon();
            
            // Store preference
            localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
        } else {
            // Mobile behavior - overlay sidebar
            const isOpen = sidebar.classList.contains('show');
            
            if (isOpen) {
                sidebar.classList.remove('show');
                if (sidebarOverlay) sidebarOverlay.classList.remove('show');
                document.body.style.overflow = '';
            } else {
                sidebar.classList.add('show');
                if (sidebarOverlay) sidebarOverlay.classList.add('show');
                document.body.style.overflow = 'hidden';
            }
            
            // Ensure header is positioned correctly for mobile
            if (topHeader) {
                topHeader.style.left = '0';
            }
        }
        
        // Announce to screen readers
        const isCollapsed = sidebar.classList.contains('collapsed') || !sidebar.classList.contains('show');
        announceToScreenReader(isCollapsed ? 'Sidebar collapsed' : 'Sidebar expanded');
    }
    
    // Update toggle button icon
    function updateToggleIcon() {
        const isCollapsed = sidebar.classList.contains('collapsed');
        const icon = sidebarToggle.querySelector('svg');
        
        if (isCollapsed) {
            // Show "expand" icon
            icon.innerHTML = `
                <path d="M5 12h14"></path>
                <path d="M12 5l7 7-7 7"></path>
            `;
            sidebarToggle.setAttribute('aria-label', 'Expand Sidebar');
        } else {
            // Show "collapse" icon (hamburger)
            icon.innerHTML = `
                <line x1="3" y1="6" x2="21" y2="6"></line>
                <line x1="3" y1="12" x2="21" y2="12"></line>
                <line x1="3" y1="18" x2="21" y2="18"></line>
            `;
            sidebarToggle.setAttribute('aria-label', 'Collapse Sidebar');
        }
    }
    
    // Restore user preference on large screens
    function restorePreference() {
        const isLargeScreen = window.innerWidth > 992;
        const topHeader = document.querySelector('.top-header');
        
        if (isLargeScreen) {
            const savedState = localStorage.getItem('sidebarCollapsed');
            if (savedState === 'true') {
                sidebar.classList.add('collapsed');
                mainContent.classList.add('expanded');
                if (topHeader) {
                    topHeader.style.left = '0';
                }
            } else {
                if (topHeader) {
                    topHeader.style.left = 'var(--sidebar-width)';
                }
            }
            updateToggleIcon();
        } else {
            // Mobile - header always spans full width
            if (topHeader) {
                topHeader.style.left = '0';
            }
        }
    }
    
    // Event listeners
    sidebarToggle.addEventListener('click', toggleSidebar);
    
    // Close mobile sidebar when overlay is clicked
    if (sidebarOverlay) {
        sidebarOverlay.addEventListener('click', function() {
            sidebar.classList.remove('show');
            sidebarOverlay.classList.remove('show');
            document.body.style.overflow = '';
        });
    }
    
    // Handle window resize
    window.addEventListener('resize', function() {
        setInitialState();
        restorePreference();
    });
    
    // Keyboard navigation
    sidebarToggle.addEventListener('keydown', function(e) {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            toggleSidebar();
        }
    });
    
    // Close mobile sidebar when pressing Escape
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && window.innerWidth <= 992) {
            if (sidebar.classList.contains('show')) {
                sidebar.classList.remove('show');
                if (sidebarOverlay) sidebarOverlay.classList.remove('show');
                document.body.style.overflow = '';
                sidebarToggle.focus();
            }
        }
    });
    
    // Initialize
    setInitialState();
    restorePreference();
}

// Removed theme toggle functionality - simplified for light theme only

// Animation System
function initializeAnimations() {
    // Fade in elements on page load
    const fadeElements = document.querySelectorAll('.fade-in');
    fadeElements.forEach((element, index) => {
        element.style.opacity = '0';
        element.style.transform = 'translateY(20px)';
        
        setTimeout(() => {
            element.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
            element.style.opacity = '1';
            element.style.transform = 'translateY(0)';
        }, index * 100);
    });

    // Slide up animations
    const slideElements = document.querySelectorAll('.slide-up');
    slideElements.forEach((element, index) => {
        element.style.opacity = '0';
        element.style.transform = 'translateY(30px)';
        
        setTimeout(() => {
            element.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
            element.style.opacity = '1';
            element.style.transform = 'translateY(0)';
        }, (index * 150) + 200);
    });

    // Intersection Observer for scroll animations
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-in');
            }
        });
    }, observerOptions);

    // Observe elements for scroll animation
    document.querySelectorAll('.observe-animation').forEach(el => {
        observer.observe(el);
    });
}

// Enhanced Interactions
function initializeInteractions() {
    // Button hover effects
    const buttons = document.querySelectorAll('.btn');
    buttons.forEach(button => {
        button.addEventListener('mouseenter', function() {
            if (!this.disabled && !this.classList.contains('loading')) {
                this.style.transform = 'translateY(-2px)';
                this.style.transition = 'all 0.2s ease';
            }
        });

        button.addEventListener('mouseleave', function() {
            if (!this.disabled && !this.classList.contains('loading')) {
                this.style.transform = 'translateY(0)';
            }
        });

        // Add ripple effect
        button.addEventListener('click', function(e) {
            if (this.disabled || this.classList.contains('loading')) return;
            
            const ripple = document.createElement('span');
            const rect = this.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
            const y = e.clientY - rect.top - size / 2;
            
            ripple.style.cssText = `
                position: absolute;
                border-radius: 50%;
                transform: scale(0);
                animation: ripple 0.6s linear;
                background-color: rgba(255, 255, 255, 0.7);
                left: ${x}px;
                top: ${y}px;
                width: ${size}px;
                height: ${size}px;
            `;
            
            this.style.position = 'relative';
            this.style.overflow = 'hidden';
            this.appendChild(ripple);
            
            setTimeout(() => {
                ripple.remove();
            }, 600);
        });
    });

    // Card hover effects
    const cards = document.querySelectorAll('.card, .feature-card, .stats-card');
    cards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-4px)';
            this.style.transition = 'all 0.3s ease';
            this.style.boxShadow = 'var(--shadow-lg)';
        });

        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
            this.style.boxShadow = '';
        });
    });

    // Table row hover effects
    const tableRows = document.querySelectorAll('.table tbody tr');
    tableRows.forEach(row => {
        row.addEventListener('mouseenter', function() {
            this.style.transform = 'scale(1.01)';
            this.style.transition = 'all 0.2s ease';
            this.style.zIndex = '10';
            this.style.position = 'relative';
        });

        row.addEventListener('mouseleave', function() {
            this.style.transform = 'scale(1)';
            this.style.zIndex = '';
            this.style.position = '';
        });
    });

    // Navigation link effects
    const navLinks = document.querySelectorAll('.nav-link');
    navLinks.forEach(link => {
        link.addEventListener('mouseenter', function() {
            this.style.backgroundColor = 'var(--primary-light)';
            this.style.color = 'var(--primary-color)';
            this.style.borderRadius = 'var(--border-radius)';
            this.style.transition = 'all 0.2s ease';
        });

        link.addEventListener('mouseleave', function() {
            this.style.backgroundColor = 'transparent';
            this.style.color = 'var(--text-muted)';
        });
    });

    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(link => {
        link.addEventListener('click', function(e) {
            const href = this.getAttribute('href');
            if (href !== '#' && href !== '#!') {
                const target = document.querySelector(href);
                if (target) {
                    e.preventDefault();
                    target.scrollIntoView({ 
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            }
        });
    });
}

// Responsive Utilities
function initializeResponsiveUtils() {
    // Mobile menu toggle
    const mobileToggle = document.querySelector('[data-bs-toggle="collapse"]');
    const mobileNav = document.getElementById('mobileNav');
    
    if (mobileToggle && mobileNav) {
        mobileToggle.addEventListener('click', function() {
            const isExpanded = this.getAttribute('aria-expanded') === 'true';
            this.setAttribute('aria-expanded', !isExpanded);
            
            // Animate hamburger icon
            const lines = this.querySelectorAll('line');
            if (lines.length >= 3) {
                if (!isExpanded) {
                    lines[0].style.transform = 'rotate(45deg) translate(5px, 5px)';
                    lines[1].style.opacity = '0';
                    lines[2].style.transform = 'rotate(-45deg) translate(7px, -6px)';
                } else {
                    lines[0].style.transform = 'none';
                    lines[1].style.opacity = '1';
                    lines[2].style.transform = 'none';
                }
            }
        });
    }

    // Dynamic viewport height fix for mobile
    function setVH() {
        const vh = window.innerHeight * 0.01;
        document.documentElement.style.setProperty('--vh', `${vh}px`);
    }
    
    setVH();
    window.addEventListener('resize', setVH);

    // Responsive table wrapper
    const tables = document.querySelectorAll('.table');
    tables.forEach(table => {
        if (!table.parentElement.classList.contains('table-responsive')) {
            const wrapper = document.createElement('div');
            wrapper.classList.add('table-responsive');
            table.parentNode.insertBefore(wrapper, table);
            wrapper.appendChild(table);
        }
    });

    // Handle window resize for responsive adjustments
    let resizeTimeout;
    window.addEventListener('resize', function() {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(function() {
            // Trigger custom resize event
            window.dispatchEvent(new CustomEvent('optimizedResize'));
        }, 250);
    });
}

// Loading States
function initializeLoadingStates() {
    // Form submission loading
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn && this.checkValidity()) {
                setLoadingState(submitBtn);
            }
        });
    });

    // AJAX link loading
    const ajaxLinks = document.querySelectorAll('[data-loading]');
    ajaxLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            if (!this.href || this.href === '#') {
                e.preventDefault();
                return;
            }
            setLoadingState(this);
        });
    });
}

function setLoadingState(element) {
    if (element.classList.contains('loading')) return;
    
    element.classList.add('loading');
    element.disabled = true;
    
    const originalContent = element.innerHTML;
    const loadingText = element.dataset.loadingText || 'Loading...';
    
    element.innerHTML = `
        <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
        ${loadingText}
    `;

    // Store original content for restoration
    element.dataset.originalContent = originalContent;
    
    // Auto restore after 10 seconds (fallback)
    setTimeout(() => {
        restoreLoadingState(element);
    }, 10000);
}

function restoreLoadingState(element) {
    if (!element.classList.contains('loading')) return;
    
    element.classList.remove('loading');
    element.disabled = false;
    
    if (element.dataset.originalContent) {
        element.innerHTML = element.dataset.originalContent;
        delete element.dataset.originalContent;
    }
}

// Enhanced Tooltips
function initializeTooltips() {
    // Add tooltips to elements with title attributes
    const tooltipElements = document.querySelectorAll('[title]');
    tooltipElements.forEach(element => {
        const title = element.getAttribute('title');
        if (title) {
            element.removeAttribute('title');
            element.dataset.tooltip = title;
            
            const tooltip = document.createElement('div');
            tooltip.className = 'custom-tooltip';
            tooltip.textContent = title;
            tooltip.setAttribute('role', 'tooltip');
            document.body.appendChild(tooltip);
            
            element.addEventListener('mouseenter', function(e) {
                showTooltip(e, tooltip, title);
            });
            
            element.addEventListener('mouseleave', function() {
                hideTooltip(tooltip);
            });
            
            element.addEventListener('mousemove', function(e) {
                updateTooltipPosition(e, tooltip);
            });

            element.addEventListener('focus', function(e) {
                showTooltip(e, tooltip, title);
            });

            element.addEventListener('blur', function() {
                hideTooltip(tooltip);
            });
        }
    });
}

function showTooltip(event, tooltip, text) {
    tooltip.textContent = text;
    tooltip.style.display = 'block';
    tooltip.style.opacity = '0';
    tooltip.style.transform = 'translateY(10px)';
    
    updateTooltipPosition(event, tooltip);
    
    setTimeout(() => {
        tooltip.style.transition = 'opacity 0.2s ease, transform 0.2s ease';
        tooltip.style.opacity = '1';
        tooltip.style.transform = 'translateY(0)';
    }, 10);
}

function hideTooltip(tooltip) {
    tooltip.style.opacity = '0';
    tooltip.style.transform = 'translateY(10px)';
    setTimeout(() => {
        tooltip.style.display = 'none';
    }, 200);
}

function updateTooltipPosition(event, tooltip) {
    const x = event.clientX + window.scrollX;
    const y = event.clientY + window.scrollY;
    
    tooltip.style.left = (x + 10) + 'px';
    tooltip.style.top = (y - tooltip.offsetHeight - 10) + 'px';
}

// Accessibility Features
function initializeAccessibility() {
    // Add skip link
    const skipLink = document.createElement('a');
    skipLink.href = '#main';
    skipLink.className = 'skip-link';
    skipLink.textContent = 'Skip to main content';
    document.body.insertBefore(skipLink, document.body.firstChild);

    // Add main landmark if not present
    const main = document.querySelector('main');
    if (main && !main.id) {
        main.id = 'main';
    }

    // Enhance form labels
    const inputs = document.querySelectorAll('input, select, textarea');
    inputs.forEach(input => {
        if (!input.getAttribute('aria-label') && !input.getAttribute('aria-labelledby')) {
            const label = document.querySelector(`label[for="${input.id}"]`);
            if (label) {
                input.setAttribute('aria-labelledby', input.id + '-label');
                label.id = input.id + '-label';
            }
        }
    });

    // Add ARIA live region for announcements
    if (!document.getElementById('aria-live-region')) {
        const liveRegion = document.createElement('div');
        liveRegion.id = 'aria-live-region';
        liveRegion.setAttribute('aria-live', 'polite');
        liveRegion.setAttribute('aria-atomic', 'true');
        liveRegion.className = 'sr-only';
        document.body.appendChild(liveRegion);
    }

    // Keyboard navigation for dropdowns
    const dropdownToggles = document.querySelectorAll('[data-bs-toggle="dropdown"]');
    dropdownToggles.forEach(toggle => {
        toggle.addEventListener('keydown', function(e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.click();
            }
        });
    });

    // Focus management for modals and dropdowns
    document.addEventListener('shown.bs.modal', function(e) {
        const focusableElements = e.target.querySelectorAll(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );
        if (focusableElements.length) {
            focusableElements[0].focus();
        }
    });

    // Trap focus in modals
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Tab') {
            const modal = document.querySelector('.modal.show');
            if (modal) {
                trapFocus(modal, e);
            }
        }
    });
}

function trapFocus(container, event) {
    const focusableElements = container.querySelectorAll(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    
    const firstElement = focusableElements[0];
    const lastElement = focusableElements[focusableElements.length - 1];

    if (event.shiftKey) {
        if (document.activeElement === firstElement) {
            lastElement.focus();
            event.preventDefault();
        }
    } else {
        if (document.activeElement === lastElement) {
            firstElement.focus();
            event.preventDefault();
        }
    }
}

function announceToScreenReader(message) {
    const liveRegion = document.getElementById('aria-live-region');
    if (liveRegion) {
        liveRegion.textContent = message;
        setTimeout(() => {
            liveRegion.textContent = '';
        }, 1000);
    }
}

// Utility Functions
function debounce(func, wait, immediate) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            timeout = null;
            if (!immediate) func(...args);
        };
        const callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func(...args);
    };
}

function throttle(func, limit) {
    let inThrottle;
    return function() {
        const args = arguments;
        const context = this;
        if (!inThrottle) {
            func.apply(context, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    }
}

// Progress Bar Animation
function animateProgressBars() {
    const progressBars = document.querySelectorAll('.progress-bar');
    progressBars.forEach(bar => {
        const width = bar.style.width || bar.getAttribute('style')?.match(/width:\s*(\d+%)/)?.[1] || '0%';
        bar.style.width = '0%';
        
        setTimeout(() => {
            bar.style.transition = 'width 1.5s ease-in-out';
            bar.style.width = width;
        }, 500);
    });
}

// Initialize progress bars when they come into view
const progressObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            animateProgressBars();
            progressObserver.unobserve(entry.target);
        }
    });
});

document.addEventListener('DOMContentLoaded', () => {
    const progressContainers = document.querySelectorAll('.progress');
    progressContainers.forEach(container => {
        progressObserver.observe(container);
    });
});

// Performance optimization - simplified without theme detection
window.addEventListener('load', function() {
    // Remove loading classes after page is fully loaded
    document.body.classList.add('loaded');
    
    // Initialize any deferred animations
    setTimeout(() => {
        document.querySelectorAll('.defer-animation').forEach(el => {
            el.classList.add('animate-in');
        });
    }, 100);
});

// Add CSS for ripple animation - simplified without theme transitions
const style = document.createElement('style');
style.textContent = `
    @keyframes ripple {
        to {
            transform: scale(4);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);
