import { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import styles from './Header.module.css';
import Image from 'next/image';
import logo from '../public/feenix-logo.png';

const Header = () => {
    const [showDropdown, setShowDropdown] = useState(false);
    const [userName, setUserName] = useState();
    const [profilePicture, setProfilePicture] = useState();
    const [fetchCompleted, setFetchCompleted] = useState(false);
    const hasFetchedData = useRef(false);
    const router = useRouter();
    const dropdownRef = useRef(null); // Reference for the dropdown menu

    const fetchUserData = async () => {
        if (fetchCompleted || hasFetchedData.current) return;
        hasFetchedData.current = true;
        console.log("Fetching user data");

        const token = localStorage.getItem('token');
        if (!token) {
            console.warn('No token found in local storage');
            return;
        }

        try {
            const response = await fetch('https://localhost:3003/getUser', {
                method: 'GET',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`,
                },
            });

            if (!response.ok) {
                throw new Error('Failed to fetch user data');
            }

            const userData = await response.json();

            if (userData && userData.realName) {
                setUserName(userData.realName);
            } else {
                console.warn('User not found in userData:', userData);
            }

            if (userData && userData.profile_image) {
                const byteString = userData.profile_image;
                const binaryString = atob(byteString);
                const byteNumbers = new Array(binaryString.length);

                for (let i = 0; i < binaryString.length; i++) {
                    byteNumbers[i] = binaryString.charCodeAt(i);
                }

                const byteArray = new Uint8Array(byteNumbers);
                const blob = new Blob([byteArray], { type: 'image/png' });
                const imageUrl = URL.createObjectURL(blob);
                setProfilePicture(imageUrl);
            } else {
                console.log("Error getting Profile Image");
            }
            setFetchCompleted(true);
        } catch (error) {
            console.error('Error fetching user data:', error);
        }
    };

    // Handle clicks outside of the dropdown
    const handleClickOutside = (event) => {
        if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
            setShowDropdown(false); // Close the dropdown if clicked outside
        }
    };

    useEffect(() => {
        fetchUserData();

        // Set up the click event listener for closing the dropdown
        if (showDropdown) {
            document.addEventListener('mousedown', handleClickOutside);
        } else {
            document.removeEventListener('mousedown', handleClickOutside);
        }

        // Cleanup event listener on component unmount
        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, [showDropdown]); // Re-run the effect when showDropdown changes

    const handleLogout = () => {
        console.log('User logged out');
        localStorage.removeItem('token');
        router.push('/Login');
    };

    return (
        <header className={styles.header}>
            <div className={styles.logoSection}>
                <Image src={logo} onClick={() => router.push('/Home')} priority alt="Logo" />
            </div>
            <div className={styles.userSection}>
                <span className={styles.greeting}>
                    Hello, <strong>{userName}</strong>
                </span>
                <img
                    key={profilePicture}
                    src={profilePicture}
                    width={64}
                    height={64}
                    className={styles.profileImage}
                    alt=""
                    onClick={() => setShowDropdown(prev => !prev)} // Toggle dropdown on image click
                    onLoad={() => URL.revokeObjectURL(profilePicture)}
                />
                {showDropdown && (
                    <div ref={dropdownRef} className={styles.logoutMenu}>
                        <button onClick={handleLogout} className={styles.logoutButton}>
                            Logout
                        </button>
                    </div>
                )}
            </div>
        </header>
    );
};

export default Header;
