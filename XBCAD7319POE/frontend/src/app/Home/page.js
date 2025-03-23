"use client";

import React, { useEffect, useState } from 'react';
import Header from '@/components/Header';
import styles from './Home.module.css';
import { useRouter } from 'next/navigation';
import ADimg from '@/public/AD.png';
import SSSCimg from '@/public/SSSC.png';
import BSSCimg from '@/public/BSSC.png';
import SSRimg from '@/public/SSR.png';
import BCHimg from '@/public/BCH.png';
import Bttm from '@/public/P1Splice.png';
import Image from 'next/image';

const HomePage = () => {
    const router = useRouter();

    useEffect(() => {
        router.prefetch('./SingleStudentCheck'); 
        router.prefetch('./BulkStudentSurveyCheck');
        router.prefetch('./AdminDashboard');
        router.prefetch('./BulkCheckHistory');
        router.prefetch('./StudentSurveyReport');
    })
    
    // Navigation functions for each button
    const handleSSSCheck = () => {
        router.push('./SingleStudentCheck'); // Redirect to the Single Student Survey page
    };

    const handleBSSCheck = () => {
        router.push('./BulkStudentSurveyCheck'); // Redirect to the Bulk Student Survey page
    };

    const handleAdminDashboard = () => {
        router.push('./AdminDashboard'); // Redirect to the Admin Dashboard page
    };

    const handleBulkCheckHistory = () => {
        router.push('./BulkCheckHistory'); // Redirect to the Bulk Check History page
    };

    const handleBulkCheckReport = () => {
        router.push('./StudentSurveyReport'); // Redirect to the Bulk Check Report page
    };

    return (
        <div className={styles.mainContainer}>
            <Header />
            <div className={styles.container}>
                {/* Single Student Survey Completion */}
                <div className={styles.card}>
                        <Image src={SSSCimg} priority className={styles.cardIcon} alt="SSSC Icon" />
                        <h3 className={styles.cardTitle}>Single Student Survey Completion Check</h3>
                        <p className={styles.cardDescription}>
                            You can perform a Single Survey Completion check by clicking the button below.
                        </p>
                        <button className={`${styles.cardButton} ${styles.SSSC}`} onClick={handleSSSCheck}>
                            Perform SSSC Check
                        </button>
                </div>

                {/* Bulk Student Survey Completion */}
                <div className={styles.card}>
                    <Image src={BSSCimg} priority className={styles.cardIcon} alt="BSSC Icon" />
                    <h3 className={styles.cardTitle}>Bulk Student Survey Completion Check</h3>
                    <p className={styles.cardDescription}>
                        You can perform a Bulk Survey Completion check by clicking the button below.
                    </p>
                    <button className={`${styles.cardButton} ${styles.BSSC}`} onClick={handleBSSCheck}>
                        Perform BSSC Check
                    </button>
                </div>

                {/* Admin Dashboard */}
                <div className={styles.card}>
                    <Image src={ADimg} priority className={styles.cardIcon} alt="Admin Dashboard Icon" />
                    <h3 className={styles.cardTitle}>Admin Dashboard</h3>
                    <p className={styles.cardDescription}>
                        You can view the admin dashboard by clicking the button below.
                    </p>
                    <button className={`${styles.cardButton} ${styles.AD}`} onClick={handleAdminDashboard}>
                        View Admin Dashboard
                    </button>
                </div>

                <div className={styles.card}>
                    <Image src={BCHimg} priority className={styles.cardIcon} alt="BCH Icon" />
                    <h3 className={styles.cardTitle}>Bulk Check History</h3>
                    <p className={styles.cardDescription}>
                        You can view all previously checked files by clicking the button below.
                    </p>
                    <button className={`${styles.cardButton} ${styles.BCH}`} onClick={handleBulkCheckHistory}>
                        View Bulk Check History
                    </button>
                </div>

                <div className={styles.card}>
                    <Image src={SSRimg} priority className={styles.cardIcon} alt="BCR Icon" />
                    <h3 className={styles.cardTitle}>Student Survey Report</h3>
                    <p className={styles.cardDescription}>
                        You can perform view a summarized report of all the completion statuses by clicking the button below.
                    </p>
                    <button className={`${styles.cardButton} ${styles.SSR}`} onClick={handleBulkCheckReport}>
                        View Student Survey Report
                    </button>
                </div>
            </div>
            <Image src={Bttm} className={styles.bottomImage} alt="Bottom Image" />
        </div>
    );
}

export default HomePage;
