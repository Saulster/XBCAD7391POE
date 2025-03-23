// tokenService.js

// Function to refresh the token
export const refreshToken = async () => {
    const token = localStorage.getItem('token'); // Get the current token if needed
    const refreshToken = localStorage.getItem('refreshToken'); // Get the refresh token

    // Check if tokens are available
    if (!token || !refreshToken) {
        console.error('No token or refresh token available.');
        return;
    }

    try {
        const response = await fetch('https://localhost:3003/api/Login/refresh-token', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}` // Send the current token for verification
            },
            body: JSON.stringify({ RefreshToken: refreshToken }) // Send the refresh token in the body
        });

        // Handle response
        if (!response.ok) {
            const errorData = await response.json();
            console.error('Failed to refresh token:', errorData.message || 'Unexpected error');
            // Optionally, handle specific error cases
            if (response.status === 401) {
                // Handle unauthorized access (e.g., redirect to login)
                // You might want to clear tokens and redirect here
                localStorage.removeItem('token');
                localStorage.removeItem('refreshToken');
                window.location.href = '/Login'; // Redirect to login page
            }
            return;
        }

        const data = await response.json();
        localStorage.setItem('token', data.token); // Store the new access token
        localStorage.setItem('refreshToken', data.refreshToken); // Store the new refresh token
        console.log('Token refreshed successfully');

    } catch (error) {
        console.error('Error refreshing token:', error);
        // Handle network or unexpected errors
    }
};
