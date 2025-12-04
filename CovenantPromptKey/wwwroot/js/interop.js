/**
 * CovenantPromptKey JavaScript Interop
 * Provides browser interop functionality for Blazor Server application
 */

window.CovenantPromptKey = {
    /**
     * Copy text to clipboard
     * @param {string} text - Text to copy
     * @returns {Promise<boolean>} - Success status
     */
    copyToClipboard: async function (text) {
        // 優先使用 navigator.clipboard API（現代瀏覽器標準）
        try {
            await navigator.clipboard.writeText(text);
            console.log('Text copied using navigator.clipboard');
            return true;
        } catch (err) {
            console.warn('navigator.clipboard failed:', err);
        }
        
        // Fallback: 使用 execCommand（已 deprecated，但作為備案）
        try {
            const textArea = document.createElement('textarea');
            textArea.value = text;
            textArea.style.position = 'fixed';
            textArea.style.top = '0';
            textArea.style.left = '0';
            textArea.style.width = '2em';
            textArea.style.height = '2em';
            textArea.style.padding = '0';
            textArea.style.border = 'none';
            textArea.style.outline = 'none';
            textArea.style.boxShadow = 'none';
            textArea.style.background = 'transparent';
            document.body.appendChild(textArea);
            textArea.focus();
            textArea.select();
            
            const successful = document.execCommand('copy');
            document.body.removeChild(textArea);
            
            if (successful) {
                console.log('Text copied using execCommand fallback');
                return true;
            }
        } catch (fallbackErr) {
            console.error('All copy methods failed:', fallbackErr);
        }
        
        return false;
    },

    /**
     * Read text from clipboard
     * @returns {Promise<string>} - Clipboard text
     */
    readFromClipboard: async function () {
        try {
            return await navigator.clipboard.readText();
        } catch (err) {
            console.error('Failed to read from clipboard:', err);
            return '';
        }
    },

    /**
     * Sync scroll position between textarea, highlight backdrop and line numbers
     * @param {string} textAreaId - Textarea element ID
     * @param {string} highlightBackdropId - Highlight backdrop element ID
     * @param {string} lineNumbersId - Line numbers container ID
     */
    syncEditorScroll: function (textAreaId, highlightBackdropId, lineNumbersId) {
        const textArea = document.getElementById(textAreaId);
        if (!textArea) return;

        const scrollTop = textArea.scrollTop;
        const scrollLeft = textArea.scrollLeft;

        // Sync highlight backdrop
        if (highlightBackdropId) {
            const backdrop = document.getElementById(highlightBackdropId);
            if (backdrop) {
                backdrop.scrollTop = scrollTop;
                backdrop.scrollLeft = scrollLeft;
            }
        }

        // Sync line numbers
        if (lineNumbersId) {
            const lineNumbers = document.getElementById(lineNumbersId);
            if (lineNumbers) {
                lineNumbers.scrollTop = scrollTop;
            }
        }
    },

    /**
     * Scroll element to specific line (1-based) with sync to line numbers
     * @param {string} elementId - Text area or display element ID
     * @param {number} lineNumber - Line number (1-based)
     * @param {string} lineNumbersId - Line numbers container ID for sync scrolling
     */
    scrollToLine: function (elementId, lineNumber, lineNumbersId) {
        const element = document.getElementById(elementId);
        if (!element) return;

        const lineHeight = parseFloat(getComputedStyle(element).lineHeight) || 21;
        const scrollTop = Math.max(0, (lineNumber - 1) * lineHeight);
        
        // Smooth scroll the main text area
        element.scrollTo({
            top: scrollTop,
            behavior: 'smooth'
        });
        
        // Sync scroll line numbers if provided
        if (lineNumbersId) {
            const lineNumbers = document.getElementById(lineNumbersId);
            if (lineNumbers) {
                lineNumbers.scrollTo({
                    top: scrollTop,
                    behavior: 'smooth'
                });
                
                // Flash the line number
                const lineNumElement = lineNumbers.querySelector(`[data-line="${lineNumber}"]`);
                if (lineNumElement) {
                    lineNumElement.classList.add('flash');
                    setTimeout(() => {
                        lineNumElement.classList.remove('flash');
                    }, 1500);
                }
            }
        }
    },

    /**
     * Scroll element into view
     * @param {string} elementId - Element ID
     */
    scrollIntoView: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollIntoView({
                behavior: 'smooth',
                block: 'center'
            });
        }
    },

    /**
     * Scroll to a specific element and flash highlight it
     * @param {string} elementId - Element ID to scroll to
     */
    scrollToElement: function (elementId) {
        const element = document.getElementById(elementId);
        if (!element) {
            console.warn('Element not found:', elementId);
            return;
        }

        // Scroll the element into view
        element.scrollIntoView({
            behavior: 'smooth',
            block: 'center'
        });
        
        // Add flash highlight effect
        element.classList.add('flash-highlight');
        setTimeout(() => {
            element.classList.remove('flash-highlight');
        }, 2000);
    },

    /**
     * Flash highlight effect on element
     * @param {string} elementId - Element ID
     * @param {number} duration - Duration in milliseconds
     */
    flashHighlight: function (elementId, duration = 500) {
        const element = document.getElementById(elementId);
        if (!element) return;

        element.classList.add('flash-highlight');
        setTimeout(() => {
            element.classList.remove('flash-highlight');
        }, duration);
    },

    /**
     * Focus element
     * @param {string} elementId - Element ID
     */
    focusElement: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.focus();
        }
    },

    /**
     * Download text as file
     * @param {string} filename - File name
     * @param {string} content - File content
     * @param {string} mimeType - MIME type (default: text/csv)
     */
    downloadFile: function (filename, content, mimeType = 'text/csv') {
        const blob = new Blob([content], { type: mimeType });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    },

    /**
     * Read file content as text
     * @param {File} file - File object from input
     * @returns {Promise<string>} - File content
     */
    readFileAsText: function (file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result);
            reader.onerror = () => reject(reader.error);
            reader.readAsText(file);
        });
    },

    /**
     * Set up keyboard shortcuts
     * @param {object} dotNetReference - .NET object reference for callbacks
     */
    setupKeyboardShortcuts: function (dotNetReference) {
        document.addEventListener('keydown', async function (e) {
            // Ctrl+Enter: Execute mask/restore
            if (e.ctrlKey && e.key === 'Enter') {
                e.preventDefault();
                await dotNetReference.invokeMethodAsync('OnCtrlEnterPressed');
            }
            // Escape: Clear/Cancel
            else if (e.key === 'Escape') {
                await dotNetReference.invokeMethodAsync('OnEscapePressed');
            }
            // Ctrl+A in editor: Select all keywords (not browser select all)
            else if (e.ctrlKey && e.key === 'a' && e.target.classList.contains('keyword-select-area')) {
                e.preventDefault();
                await dotNetReference.invokeMethodAsync('OnSelectAllPressed');
            }
        });
    },

    /**
     * Remove keyboard shortcuts
     */
    removeKeyboardShortcuts: function () {
        // Note: Would need to store reference to handler to remove it properly
        // For now, this is a placeholder
    },

    /**
     * Get scroll position of element
     * @param {string} elementId - Element ID
     * @returns {object} - Scroll position {top, left}
     */
    getScrollPosition: function (elementId) {
        const element = document.getElementById(elementId);
        if (!element) return { top: 0, left: 0 };
        return {
            top: element.scrollTop,
            left: element.scrollLeft
        };
    },

    /**
     * Set scroll position of element
     * @param {string} elementId - Element ID
     * @param {number} top - Scroll top position
     * @param {number} left - Scroll left position
     */
    setScrollPosition: function (elementId, top, left) {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollTop = top;
            element.scrollLeft = left;
        }
    }
};

/**
 * Global helper functions for JS interop
 */

/**
 * Download content as file (global wrapper)
 * @param {string} filename - File name
 * @param {string} content - File content  
 * @param {string} mimeType - MIME type
 */
window.downloadFile = function (filename, content, mimeType) {
    window.CovenantPromptKey.downloadFile(filename, content, mimeType || 'text/csv');
};

/**
 * Copy to clipboard (global wrapper)
 * @param {string} text - Text to copy
 * @returns {Promise<boolean>} - Success status
 */
window.copyToClipboard = async function (text) {
    return await window.CovenantPromptKey.copyToClipboard(text);
};

/**
 * Read file content from file input element
 * @param {HTMLInputElement} fileInput - File input element reference
 * @returns {Promise<string>} - File content as text
 */
window.readFileContent = async function (fileInput) {
    if (!fileInput || !fileInput.files || fileInput.files.length === 0) {
        return '';
    }
    
    const file = fileInput.files[0];
    return await window.CovenantPromptKey.readFileAsText(file);
};

/**
 * Keyboard shortcut handler
 * Registers global keyboard shortcuts and invokes Blazor callbacks
 */
window.CovenantPromptKey.keyboardShortcuts = {
    _dotNetReference: null,
    _isRegistered: false,

    /**
     * Register keyboard shortcut handler
     * @param {DotNetObjectReference} dotNetRef - Reference to Blazor component
     */
    register: function (dotNetRef) {
        this._dotNetReference = dotNetRef;
        
        if (!this._isRegistered) {
            document.addEventListener('keydown', this._handleKeydown.bind(this));
            this._isRegistered = true;
        }
    },

    /**
     * Unregister keyboard shortcut handler
     */
    unregister: function () {
        if (this._isRegistered) {
            document.removeEventListener('keydown', this._handleKeydown.bind(this));
            this._isRegistered = false;
        }
        this._dotNetReference = null;
    },

    /**
     * Handle keydown event
     * @param {KeyboardEvent} event - Keyboard event
     */
    _handleKeydown: async function (event) {
        // Skip if typing in an input or textarea (except for certain keys)
        const isTyping = event.target.tagName === 'INPUT' || 
                        event.target.tagName === 'TEXTAREA' ||
                        event.target.isContentEditable;
        
        // Handle Escape - always active
        if (event.key === 'Escape') {
            if (this._dotNetReference) {
                await this._dotNetReference.invokeMethodAsync('OnEscapePressed');
            }
            return;
        }
        
        // Skip other shortcuts if typing (except Ctrl+Enter)
        if (isTyping && !(event.ctrlKey && event.key === 'Enter')) {
            return;
        }
        
        // Ctrl+A - Select all keywords
        if (event.ctrlKey && event.key === 'a' && !event.shiftKey && !event.altKey) {
            event.preventDefault();
            if (this._dotNetReference) {
                await this._dotNetReference.invokeMethodAsync('OnSelectAllPressed');
            }
            return;
        }
        
        // Ctrl+Enter - Execute (mask/restore)
        if (event.ctrlKey && event.key === 'Enter' && !event.shiftKey && !event.altKey) {
            event.preventDefault();
            if (this._dotNetReference) {
                await this._dotNetReference.invokeMethodAsync('OnExecutePressed');
            }
            return;
        }
    }
};

// Create alias for interop calls
window.interop = window.CovenantPromptKey;
