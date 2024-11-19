function validateAndUpdateInput(element) {
    // Sanitize input
    let text = element.value;
    
    // Remove HTML tags and dangerous characters
    text = text.replace(/<[^>]*>/g, '');
    text = text.replace(/[<>{}\\]/g, '');
    
    if (text !== element.value) {
        element.value = text;
    }

    // Update character count
    const charCount = text.length;
    document.getElementById('charCount').textContent = charCount + "/160";

    // Auto-resize textarea
    element.style.height = 'auto';
    element.style.height = element.scrollHeight + 'px';

    // Visual feedback for length
    if (charCount > 160) {
        element.style.borderColor = 'red';
    } else {
        element.style.borderColor = '';
    }
}

// Prevent paste events from containing HTML
document.addEventListener('DOMContentLoaded', function() {
    const cheepInput = document.getElementById('cheepInput');
    if (cheepInput) {
        cheepInput.addEventListener('paste', function(e) {
            e.preventDefault();
            const text = (e.originalEvent || e).clipboardData.getData('text/plain');
            document.execCommand('insertText', false, text);
        });
    }
});