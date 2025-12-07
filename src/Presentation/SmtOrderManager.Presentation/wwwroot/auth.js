window.smtAuth = {
    login: async function (payload) {
        try {
            const response = await fetch('/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload),
                credentials: 'include'
            });

            if (!response.ok) {
                return {
                    success: false,
                    errorMessage: 'Login failed.'
                };
            }

            const user = await response.json();
            return {
                success: true,
                user
            };
        } catch (error) {
            return {
                success: false,
                errorMessage: error?.message ?? 'Unexpected error during login.'
            };
        }
    }
};
