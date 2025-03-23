'use client';

import { useState, useEffect } from 'react';
import Header from '@/components/Header'; // Adjust the import path as necessary
import styles from './SSSC.module.css';
import Footer from '@/components/Footer';
import background from '@/public/Pattern 2.png';
import Image from 'next/image';
import { useRouter } from 'next/navigation';

const SSSC = () => {
    const [studentId, setStudentId] = useState(''); // State for student ID
    const router = useRouter();
    useEffect(() => {

    }, []);

    const handleSubmit = async (event) => {
        event.preventDefault(); // Prevent default form submission

        // Validate input
        if (!studentId) {
            // setErrorMessage('Student ID cannot be empty.');
            return;
        }

        if (studentId.length < 3) {
           // setErrorMessage('Student ID must be at least 3 characters long.');
            return;
        }

        // Data is valid, send to backend
        try {
            const response = await fetch(`https://localhost:3003/api/Student/singleCheck/${studentId}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                },
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const data = await response.json();
            console.log('Success:', data);
            setStudentId(''); // Clear input after successful submission
           
            //After Check Code
            router.push(`/AdminDashboard?sID=${studentId}`);
        } catch (error) {
            alert('Error sending data: ' + error.message);
        }
    }

    return (
            <div className={styles.container}>
                <Header />
                <main className={styles.main}>
                    <div className={styles.backgroundImage}>
                        <div className={styles.heading}>
                            <strong>Single Student Survey Completion Check</strong>
                        </div>
                    </div>
                    <form onSubmit={handleSubmit} className={styles.formContainer}>
                        <div className={styles.form}>
                            <div className={styles.inputGroup}>
                                <label htmlFor="studentId" className={styles.label}>
                                    Enter Student ID*
                                </label>
                                <input
                                    type="text"
                                    id="studentId"
                                    name="studentId"
                                    value={studentId}
                                    onChange={(e) => setStudentId(e.target.value)}
                                    className={styles.input}
                                    placeholder="Student ID*"
                                    required
                                />
                            </div>
                            <button type="submit" className={styles.submitButton}>
                                Next
                            </button>
                        </div>
                    </form>
                </main>
                <Footer />
            </div>
        );
    };

    export default SSSC;
