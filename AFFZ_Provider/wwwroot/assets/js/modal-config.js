// Modal Configuration - Prevents modals from being hidden unless user clicks cancel
$(document).ready(function() {
    // Configure all existing modals
    configureModals();
    
    // Configure modals that might be added dynamically
    $(document).on('shown.bs.modal', function(e) {
        configureModal($(e.target));
    });
    
    // Also configure modals when they are about to be shown
    $(document).on('show.bs.modal', function(e) {
        configureModal($(e.target));
    });
});

function configureModals() {
    // Configure all existing modals
    $('.modal').each(function() {
        configureModal($(this));
    });
}

function configureModal(modalElement) {
    if (!modalElement.length) return;
    
    // Set backdrop to static to prevent closing when clicking outside (Bootstrap 4)
    modalElement.attr('data-backdrop', 'static');
    
    // Disable keyboard (Escape key) closing (Bootstrap 4)
    modalElement.attr('data-keyboard', 'false');
    
    // Remove data-dismiss from close buttons that aren't cancel buttons (Bootstrap 4)
    modalElement.find('[data-bs-dismiss="modal"]').each(function() {
        var $button = $(this);
        var buttonText = $button.text().toLowerCase().trim();
        var buttonClass = $button.attr('class') || '';
        
        // Check if this is a cancel button - include close button (×) as cancel
        var isCancelButton = buttonText.includes('cancel') || 
                           buttonText.includes('×') ||
                           buttonClass.includes('btn-secondary') ||
                           buttonClass.includes('close-btn') ||
                           buttonClass.includes('close'); // Include close class as cancel
        
        // If it's not a cancel button, remove the dismiss attribute
        if (!isCancelButton) {
            $button.removeAttr('data-dismiss');
        }
    });
    
    // Add click handlers to non-cancel close buttons to prevent closing
    modalElement.find('.btn-close').each(function() {
        var $button = $(this);
        if (!$button.attr('data-dismiss')) {
            $button.off('click.modal-prevent').on('click.modal-prevent', function(e) {
                e.preventDefault();
                e.stopPropagation();
                return false;
            });
        }
    });
    
    // Force modal options to be set
    try {
        var modalInstance = modalElement.data('bs.modal');
        if (modalInstance) {
            modalInstance.options.backdrop = 'static';
            modalInstance.options.keyboard = false;
        }
    } catch (e) {
        // Modal instance might not be available yet
    }
}

// Override Bootstrap modal hide behavior for non-cancel actions (Bootstrap 4)
$(document).on('hide.bs.modal', function(e) {
    var $modal = $(e.target);
    var $trigger = $(e.relatedTarget);
    
    // Allow closing only if:
    // 1. It's triggered by a cancel button
    // 2. It's triggered by a button with data-bs-dismiss="modal"
    // 3. It's programmatically called
    
    if ($trigger.length && $trigger.attr('data-dismiss') === 'modal') {
        return; // Allow closing
    }
    
    // Check if the trigger is a cancel button
    if ($trigger.length) {
        var buttonText = $trigger.text().toLowerCase().trim();
        var buttonClass = $trigger.attr('class') || '';
        
        var isCancelButton = buttonText.includes('cancel') || 
                           buttonText.includes('×') ||
                           buttonClass.includes('btn-secondary') ||
                           buttonClass.includes('close-btn') ||
                           buttonClass.includes('close'); // Include close class as cancel
        
        if (isCancelButton) {
            return; // Allow closing
        }
    }
    
    // Prevent closing for all other cases
    e.preventDefault();
    return false;
});

// Function to manually close a modal (for cancel actions) - Bootstrap 4
function closeModal(modalId) {
    $('#' + modalId).modal('hide');
}

// Function to manually close a modal by element - Bootstrap 4
function closeModalElement(modalElement) {
    $(modalElement).modal('hide');
}
