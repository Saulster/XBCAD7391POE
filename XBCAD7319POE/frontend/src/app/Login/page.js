"use client";
import './LoginPageDesign.module.css';
import styles from './LoginPageDesign.module.css';
import { useState } from 'react';
import { useRouter } from 'next/navigation';

import axios from 'axios';
import jwt from 'jsonwebtoken';

function LoginPage() {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [token, setToken] = useState('');
    const router = useRouter();

    const secretKey = process.env.JWT_SECRET;

    const handleLogin = async (e) => {
        e.preventDefault(); // Prevent the default form submission

        try {
            // API call to authenticate the user
            const response = await fetch('https://localhost:3003/api/Login/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ uName: username, Password: password }), // Send username and password
            });

            if (response.status === 401) {
                alert('Invalid Credentials');
                return;
            }
            else if (!response.ok) {
                const errorData = await response.json();
                console.error('Login failed:', errorData.message || 'Unexpected error');
                alert('Login failed:', errorData.message || 'Unexpected error');
                return; // Exit if login failed
            }

            const data = await response.json(); // Expecting the backend to return the JWT token
            localStorage.setItem('token', data.token); // Store the token in local storage

            console.log('Logged in successfully:', jwt.decode(data.token)); // Decode the token if needed
            router.push('/Home'); // Redirect to the home page
        } catch (error) {
            console.error('Error during login:', error);
        }
    };

    return (
        <div className={styles.pageContainer}>
            <div className={styles.loginContainer}>
                <h2>Feenix</h2>
                <form>
                    <label htmlFor="username">Username &#128100;</label>
                    <input
                        type="text"
                        id="username"
                        name="username"
                        placeholder="Enter your username"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                    />

                    <label htmlFor="password">Password &#128274;</label>
                    <input
                        type="password"
                        id="password"
                        name="password"
                        placeholder="Enter your password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />

                    <button onClick={handleLogin}>Login</button>
                </form>
            </div>
        </div>
    );
};

export default LoginPage;