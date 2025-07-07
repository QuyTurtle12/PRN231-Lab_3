// Toast notification functions
const ToastManager = {
    // Show a toast notification
    show: function (message, type = 'success') {
        // Create toast container if it doesn't exist
        let toastContainer = document.getElementById('toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = 'toast-container';
            toastContainer.className = 'position-fixed bottom-0 end-0 p-3';
            toastContainer.style.zIndex = '11';
            document.body.appendChild(toastContainer);
        }

        // Create unique ID for this toast
        const toastId = 'toast-' + Date.now();

        // Create toast element
        const toast = document.createElement('div');
        toast.className = `toast align-items-center text-white bg-${type} border-0`;
        toast.id = toastId;
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');

        // Set up icon based on type
        const icon = type === 'success' ? 'bi-check-circle' :
            type === 'danger' ? 'bi-exclamation-circle' :
                type === 'warning' ? 'bi-exclamation-triangle' : 'bi-info-circle';

        // Create toast content
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    <i class="bi ${icon} me-2"></i> ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>`;

        // Add to container
        toastContainer.appendChild(toast);

        // Initialize and show the toast
        const bsToast = new bootstrap.Toast(toast, {
            delay: 5000,
            autohide: true
        });

        bsToast.show();

        // Remove from DOM when hidden
        toast.addEventListener('hidden.bs.toast', function () {
            toast.remove();
        });

        return toastId;
    },

    // Convenience methods for different toast types
    success: function (message) {
        return this.show(message, 'success');
    },

    error: function (message) {
        return this.show(message, 'danger');
    },

    warning: function (message) {
        return this.show(message, 'warning');
    },

    info: function (message) {
        return this.show(message, 'info');
    }
};