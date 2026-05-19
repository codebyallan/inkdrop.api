(function () {
    const ORIGINAL_FETCH = window.fetch;
    window.fetch = async (...args) => {
        let [resource, config] = args;
        if (config && ['POST', 'PUT', 'DELETE', 'PATCH'].includes(config.method?.toUpperCase())) {
            const xsrfToken = document.cookie.split('; ').find(row => row.startsWith('XSRF-TOKEN='))?.split('=')[1];
            if (xsrfToken) {
                config.headers = config.headers || {};
                const tokenValue = decodeURIComponent(xsrfToken);
                if (config.headers instanceof Headers) { config.headers.set('X-XSRF-TOKEN', tokenValue); }
                else if (Array.isArray(config.headers)) { config.headers.push(['X-XSRF-TOKEN', tokenValue]); }
                else { config.headers['X-XSRF-TOKEN'] = tokenValue; }
            }
        }
        return ORIGINAL_FETCH(resource, config);
    };
})();
