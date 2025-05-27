// Custom Swagger UI Enhancements

(function() {
    'use strict';

    // Wait for Swagger UI to load
    function waitForSwaggerUI() {
        if (typeof window.ui !== 'undefined') {
            initializeEnhancements();
        } else {
            setTimeout(waitForSwaggerUI, 100);
        }
    }

    function initializeEnhancements() {
        console.log('🚀 Initializing Swagger UI enhancements...');

        // Add environment indicator
        addEnvironmentIndicator();

        // Add quick navigation
        addQuickNavigation();

        // Add copy to clipboard functionality
        addCopyToClipboard();

        // Add response time tracking
        addResponseTimeTracking();

        // Add keyboard shortcuts
        addKeyboardShortcuts();

        // Add dark mode toggle
        addDarkModeToggle();

        // Add API statistics
        addApiStatistics();

        console.log('✅ Swagger UI enhancements loaded');
    }

    function addEnvironmentIndicator() {
        const topbar = document.querySelector('.swagger-ui .topbar');
        if (topbar && !document.querySelector('.env-indicator')) {
            const envIndicator = document.createElement('div');
            envIndicator.className = 'env-indicator';
            envIndicator.innerHTML = `
                <span class="env-badge ${getEnvironmentClass()}">${getEnvironmentName()}</span>
            `;
            envIndicator.style.cssText = `
                position: absolute;
                top: 10px;
                right: 20px;
                z-index: 1000;
            `;
            topbar.appendChild(envIndicator);
        }
    }

    function getEnvironmentName() {
        const hostname = window.location.hostname;
        if (hostname.includes('localhost') || hostname.includes('127.0.0.1')) {
            return 'DEV';
        } else if (hostname.includes('staging') || hostname.includes('test')) {
            return 'STAGING';
        } else {
            return 'PROD';
        }
    }

    function getEnvironmentClass() {
        const env = getEnvironmentName();
        return env === 'DEV' ? 'env-dev' : env === 'STAGING' ? 'env-staging' : 'env-prod';
    }

    function addQuickNavigation() {
        const info = document.querySelector('.swagger-ui .info');
        if (info && !document.querySelector('.quick-nav')) {
            const quickNav = document.createElement('div');
            quickNav.className = 'quick-nav';
            quickNav.innerHTML = `
                <h4>🧭 Quick Navigation</h4>
                <div class="nav-buttons">
                    <button onclick="scrollToSection('Upload')" class="nav-btn">📤 Upload</button>
                    <button onclick="scrollToSection('Download')" class="nav-btn">📥 Download</button>
                    <button onclick="scrollToSection('Search')" class="nav-btn">🔍 Search</button>
                    <button onclick="scrollToSection('Health')" class="nav-btn">🏥 Health</button>
                </div>
            `;

            const style = document.createElement('style');
            style.textContent = `
                .quick-nav {
                    background: rgba(255, 255, 255, 0.9);
                    border-radius: 8px;
                    padding: 15px;
                    margin: 20px 0;
                    border: 1px solid #e9ecef;
                }
                .nav-buttons {
                    display: flex;
                    gap: 10px;
                    flex-wrap: wrap;
                    margin-top: 10px;
                }
                .nav-btn {
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    color: white;
                    border: none;
                    border-radius: 6px;
                    padding: 8px 16px;
                    cursor: pointer;
                    font-weight: bold;
                    transition: all 0.3s ease;
                }
                .nav-btn:hover {
                    background: linear-gradient(135deg, #5a67d8 0%, #6b46c1 100%);
                    transform: translateY(-1px);
                }
                .env-badge {
                    padding: 4px 8px;
                    border-radius: 4px;
                    font-weight: bold;
                    font-size: 12px;
                }
                .env-dev { background: #28a745; color: white; }
                .env-staging { background: #ffc107; color: #212529; }
                .env-prod { background: #dc3545; color: white; }
            `;
            document.head.appendChild(style);

            info.appendChild(quickNav);
        }
    }

    // Global function for navigation
    window.scrollToSection = function(sectionName) {
        const section = Array.from(document.querySelectorAll('.opblock-tag'))
            .find(tag => tag.textContent.includes(sectionName));
        if (section) {
            section.scrollIntoView({ behavior: 'smooth', block: 'start' });
            // Add visual feedback
            section.style.boxShadow = '0 0 20px rgba(102, 126, 234, 0.5)';
            setTimeout(() => {
                section.style.boxShadow = '';
            }, 2000);
        }
    };

    function addCopyToClipboard() {
        // Add copy buttons to code blocks
        const style = document.createElement('style');
        style.textContent = `
            .copy-btn {
                position: absolute;
                top: 10px;
                right: 10px;
                background: rgba(0, 0, 0, 0.7);
                color: white;
                border: none;
                border-radius: 4px;
                padding: 4px 8px;
                cursor: pointer;
                font-size: 12px;
                opacity: 0;
                transition: opacity 0.3s ease;
            }
            .copy-container:hover .copy-btn {
                opacity: 1;
            }
            .copy-btn:hover {
                background: rgba(0, 0, 0, 0.9);
            }
        `;
        document.head.appendChild(style);

        // Observer to add copy buttons to new code blocks
        const observer = new MutationObserver(() => {
            document.querySelectorAll('.highlight-code:not(.copy-enhanced)').forEach(addCopyButton);
        });
        observer.observe(document.body, { childList: true, subtree: true });
    }

    function addCopyButton(codeBlock) {
        codeBlock.classList.add('copy-enhanced');
        codeBlock.style.position = 'relative';
        codeBlock.classList.add('copy-container');

        const copyBtn = document.createElement('button');
        copyBtn.className = 'copy-btn';
        copyBtn.textContent = '📋 Copy';
        copyBtn.onclick = () => {
            const code = codeBlock.textContent;
            navigator.clipboard.writeText(code).then(() => {
                copyBtn.textContent = '✅ Copied!';
                setTimeout(() => {
                    copyBtn.textContent = '📋 Copy';
                }, 2000);
            });
        };
        codeBlock.appendChild(copyBtn);
    }

    function addResponseTimeTracking() {
        // Track API response times
        const originalFetch = window.fetch;
        window.fetch = function(...args) {
            const startTime = performance.now();
            return originalFetch.apply(this, args)
                .then(response => {
                    const endTime = performance.now();
                    const responseTime = Math.round(endTime - startTime);

                    // Add response time to the UI
                    setTimeout(() => {
                        const responseElements = document.querySelectorAll('.response');
                        responseElements.forEach(el => {
                            if (!el.querySelector('.response-time')) {
                                const timeEl = document.createElement('div');
                                timeEl.className = 'response-time';
                                timeEl.style.cssText = `
                                    background: #e3f2fd;
                                    border: 1px solid #2196f3;
                                    border-radius: 4px;
                                    padding: 5px 10px;
                                    margin: 10px 0;
                                    font-weight: bold;
                                    color: #1976d2;
                                `;
                                timeEl.innerHTML = `⏱️ Response Time: ${responseTime}ms`;
                                el.appendChild(timeEl);
                            }
                        });
                    }, 100);

                    return response;
                });
        };
    }

    function addKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            // Ctrl/Cmd + K: Focus search
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                const searchInput = document.querySelector('.filter-container input');
                if (searchInput) {
                    searchInput.focus();
                    searchInput.select();
                }
            }

            // Escape: Close all expanded operations
            if (e.key === 'Escape') {
                document.querySelectorAll('.opblock.is-open .opblock-summary').forEach(summary => {
                    summary.click();
                });
            }
        });

        // Add keyboard shortcuts help
        const info = document.querySelector('.swagger-ui .info');
        if (info && !document.querySelector('.keyboard-shortcuts')) {
            const shortcuts = document.createElement('div');
            shortcuts.className = 'keyboard-shortcuts';
            shortcuts.innerHTML = `
                <details>
                    <summary style="cursor: pointer; font-weight: bold;">⌨️ Keyboard Shortcuts</summary>
                    <div style="margin-top: 10px; font-size: 0.9rem;">
                        <kbd>Ctrl/Cmd + K</kbd> - Focus search<br>
                        <kbd>Esc</kbd> - Close all operations<br>
                        <kbd>?</kbd> - Show this help
                    </div>
                </details>
            `;
            info.appendChild(shortcuts);
        }
    }

    function addDarkModeToggle() {
        const topbar = document.querySelector('.swagger-ui .topbar');
        if (topbar && !document.querySelector('.dark-mode-toggle')) {
            const toggle = document.createElement('button');
            toggle.className = 'dark-mode-toggle';
            toggle.innerHTML = '🌙';
            toggle.style.cssText = `
                background: rgba(255, 255, 255, 0.2);
                border: none;
                border-radius: 50%;
                width: 40px;
                height: 40px;
                color: white;
                cursor: pointer;
                font-size: 18px;
                position: absolute;
                top: 10px;
                right: 80px;
                transition: all 0.3s ease;
            `;

            toggle.onclick = () => {
                document.body.classList.toggle('dark-mode');
                toggle.innerHTML = document.body.classList.contains('dark-mode') ? '☀️' : '🌙';
            };

            topbar.appendChild(toggle);
        }
    }

    function addApiStatistics() {
        const info = document.querySelector('.swagger-ui .info');
        if (info && !document.querySelector('.api-stats')) {
            const stats = document.createElement('div');
            stats.className = 'api-stats';
            stats.innerHTML = `
                <div style="display: flex; gap: 20px; margin: 20px 0; flex-wrap: wrap;">
                    <div class="stat-card">
                        <div class="stat-number" id="total-endpoints">-</div>
                        <div class="stat-label">Total Endpoints</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-number" id="total-operations">-</div>
                        <div class="stat-label">Operations</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-number" id="total-models">-</div>
                        <div class="stat-label">Models</div>
                    </div>
                </div>
            `;

            const style = document.createElement('style');
            style.textContent = `
                .stat-card {
                    background: white;
                    border-radius: 8px;
                    padding: 15px;
                    text-align: center;
                    border: 1px solid #e9ecef;
                    min-width: 120px;
                    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                }
                .stat-number {
                    font-size: 2rem;
                    font-weight: bold;
                    color: #667eea;
                }
                .stat-label {
                    font-size: 0.9rem;
                    color: #6c757d;
                    margin-top: 5px;
                }
            `;
            document.head.appendChild(style);

            info.appendChild(stats);

            // Update stats
            setTimeout(updateApiStatistics, 1000);
        }
    }

    function updateApiStatistics() {
        const endpoints = document.querySelectorAll('.opblock').length;
        const operations = document.querySelectorAll('.opblock-summary').length;
        const models = document.querySelectorAll('.model-container').length;

        const totalEndpoints = document.getElementById('total-endpoints');
        const totalOperations = document.getElementById('total-operations');
        const totalModels = document.getElementById('total-models');

        if (totalEndpoints) totalEndpoints.textContent = endpoints;
        if (totalOperations) totalOperations.textContent = operations;
        if (totalModels) totalModels.textContent = models;
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', waitForSwaggerUI);
    } else {
        waitForSwaggerUI();
    }
})();