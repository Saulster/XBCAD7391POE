"use client";

import { useState, useEffect } from "react";
import styles from "./BulkCheckResult.module.css";
import { useRouter, useSearchParams } from 'next/navigation';
import Header from '@/components/Header.js';
import Footer from '@/components/Footer.js';

const BulkStudentSurveyCheckResult = () => {
    const searchParams = useSearchParams();
    const resultParam = searchParams ? searchParams.get('result') : null;  // Check for searchParams availability
    const router = useRouter();
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [Results, setResults] = useState([]);

    // Parse results and update loading state
    useEffect(() => {
        if (resultParam) {  // Ensure resultParam exists before parsing
            try {
                const parsedResults = JSON.parse(resultParam);
                setResults(parsedResults);
            } catch (err) {
                setError("Failed to parse results.");
                console.error("Parse Error:", err);
            } finally {
                setLoading(false);
            }
        } else {
            setLoading(false);
        }
    }, [resultParam]);

    const handleToDashboard = () => {
        router.push('/AdminDashboard');
    };

    const handleResultsDownload = async (event) => {
        try {
            const response = await fetch("https://localhost:3003/api/Student/resultDownload", {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(Results)
            });

            if (!response.ok) throw new Error('Network response was not ok');

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;

            const currentDate = new Date();
            link.download = `${currentDate.toISOString().split('T')[0]}_${currentDate.toTimeString().split(' ')[0].replace(/:/g, '-')}_Results.csv`;

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            window.URL.revokeObjectURL(url);

            router.push('/AdminDashboard');
        } catch (error) {
            setError('Failed to download file');
            console.error("Download Error:", error);
            alert(`Error downloading file: ${error.message}`);
        }
    };

    return (
        <div className={styles.container}>
            <Header />
            <header className={styles.header}>
                <div className={styles.title}>Bulk Student Survey Check Result</div>
            </header>

            <div className={styles.content}>
                <div className={styles.tableSection}>
                    {loading && <p>Loading...</p>}
                    {error && <p>{error}</p>}
                    {!loading && !error && (
                        <table className={styles.table}>
                            <thead>
                                <tr>
                                    <th>Student ID</th>
                                    <th>Survey Completed</th>
                                </tr>
                            </thead>
                            <tbody>
                                {Results.length > 0 ? (
                                    Results.map((result, index) => (
                                        <tr key={index} className={index % 2 === 0 ? styles.rowEven : styles.rowOdd}>
                                            <td>{result.studentId}</td>
                                            <td>{result.completed}</td>
                                        </tr>
                                    ))
                                ) : (
                                    <tr>
                                        <td colSpan="2">No results available</td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    )}
                </div>

                <div className={styles.optionsSection}>
                    <div className={styles.optionsTitle}>Options</div>
                    <button className={styles.button} onClick={handleToDashboard}>
                        Continue to Dashboard
                    </button>
                    <button className={styles.button} onClick={handleResultsDownload}>
                        &#11123; Download Result
                    </button>
                </div>
            </div>
            <Footer />
        </div>
    );
};

export default BulkStudentSurveyCheckResult;
