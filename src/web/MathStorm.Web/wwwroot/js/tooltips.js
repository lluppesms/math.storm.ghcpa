// Initialize Bootstrap tooltips
window.initializeTooltips = () => {
    // Find all elements with data-bs-toggle="tooltip" and initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    
    // Dispose of existing tooltips first to avoid duplicates
    tooltipTriggerList.forEach(function (tooltipTriggerEl) {
        const existingTooltip = bootstrap.Tooltip.getInstance(tooltipTriggerEl);
        if (existingTooltip) {
            existingTooltip.dispose();
        }
    });
    
    // Initialize new tooltips
    const tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl, {
            html: true,
            trigger: 'hover focus'
        });
    });
    
    return tooltipList.length;
};

// Initialize tooltips when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    window.initializeTooltips();
});

// Re-initialize tooltips when Blazor updates the DOM
document.addEventListener('DOMNodeInserted', function() {
    // Small delay to allow Blazor to finish updating
    setTimeout(() => {
        window.initializeTooltips();
    }, 100);
});