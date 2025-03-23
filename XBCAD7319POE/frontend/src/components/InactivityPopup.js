// components/InactivityPopup.js
import React, { useState, useEffect } from 'react';
import styles from './InactivityPopup.module.css'; // Adjust the path if necessary
import { refreshToken } from './tokenService';
import { useRouter } from 'next/navigation';

const InactivityPopup = () => {
    const [isPopupVisible, setIsPopupVisible] = useState(false);
    const [inactivityTimer, setInactivityTimer] = useState(null);
    const router = useRouter();

    // Function to show the popup
    const showPopup = () => {
        setIsPopupVisible(true);
        if (inactivityTimer) {
            clearTimeout(inactivityTimer);
        }
    };

    // Function to handle user activity
    const resetInactivityTimer = () => {
        if (inactivityTimer) {
            clearTimeout(inactivityTimer);
        }
        // Set a new inactivity timer
        setInactivityTimer(setTimeout(showPopup, 300000)); // 5 minutes
    };

    useEffect(() => {
        resetInactivityTimer(); // Start the timer on mount

        // Add event listeners for user activity
        const handleUserActivity = () => {
            resetInactivityTimer(); // Reset the timer on any user activity
        };

        // Attach event listeners
        window.addEventListener('mousemove', handleUserActivity);
        window.addEventListener('scroll', handleUserActivity);
        window.addEventListener('click', handleUserActivity);

        return () => {
            // Cleanup event listeners on component unmount
            window.removeEventListener('mousemove', handleUserActivity);
            window.removeEventListener('scroll', handleUserActivity);
            window.removeEventListener('click', handleUserActivity);
            if (inactivityTimer) {
                clearTimeout(inactivityTimer);
            }
        };
    }, [inactivityTimer]); // Dependency on inactivityTimer

    // Function to handle staying logged in
    const handleStayLoggedIn = async () => {
        try {
            await refreshToken(); // Refresh the token
            setIsPopupVisible(false); // Close the popup
            resetInactivityTimer();   // Reset the inactivity timer
        } catch (error) {
            console.error('Failed to refresh token:', error);
            // Optionally show an error message
            alert('Failed to refresh session. Please log in again.');
        }
    };

    // Function to handle logout
    const handleLogout = () => {
        console.log('User logged out');
        localStorage.removeItem('token'); // Example: clear token
        setIsPopupVisible(false); // Close the popup
        router.push('/Login');
    };

    return (
        <div>
            {isPopupVisible && (
                <div className={styles.popup}>
                    <h2>You have been inactive!</h2>
                    <p>Would you like to stay logged in?</p>
                    <button onClick={handleStayLoggedIn}>Yes</button>
                    <button onClick={handleLogout}>Logout</button>
                </div>
            )}
        </div>
    );
};

export default InactivityPopup;
