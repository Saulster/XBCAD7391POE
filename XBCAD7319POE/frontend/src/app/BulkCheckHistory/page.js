'use client';

import styles from './BulkCheckHistory.module.css';
import Header from '@/components/Header';
import { useState, useEffect } from 'react';
import Bttm from '@/public/P1Splice.png';
import Image from 'next/image';

function BulkCheckHistory() {
    const [bulkData, setBulkData] = useState([]); // Initialize with an empty array
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // Fetch data from the backend API
    useEffect(() => {
        const fetchBulkHistory = async () => {
            setLoading(true); // Set loading to true before fetching
            try {
                const response = await fetch('https://localhost:3003/api/Student/bulkHistory'); // Adjust the route if needed
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                    alert('Internal Error: Failed to fetch \'.csv\' files');
                }
                const data = await response.json();
                setBulkData(data);
            } catch (error) {
                console.error("Error fetching bulk history data:", error);
                setError(error.message); // Set error message
                alert(`Internal Error: ${error.message}`);
            } finally {
                setLoading(false); // Set loading to false after the fetch is complete
            }
        };
        fetchBulkHistory();
    }, []);

    const download = async (file) => {
        try {
            const response = await fetch(`https://localhost:3003/api/Student/bulkDownload?fileName=${file.fileName}`, {
                method: 'GET',
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
                alert(`Internal Error: Failed to download ${file.fileName}`);
            }
            try {
                // Convert the response into a blob
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.download = file.fileName; // Use the file's original name
                document.body.appendChild(link);
                link.click();

                // Clean up the temporary link
                document.body.removeChild(link);
                window.URL.revokeObjectURL(url);
            }
            catch(error) {
                throw new Error('Conversion error')
                alert(`Internal Error: Failed to downlaod ${file.fileName}`);
            }
        } catch (error) {
            alert('Error during download: ' + error.message);
        }
    };

    const handleRecheck = async (file) => {
        try {
            // Create a new FormData object
            const formData = new FormData();

            // Fetch the CSV file from the server
            const response = await fetch(`https://localhost:3003/api/Student/bulkDownload?fileName=${file.fileName}`, {
                method: 'GET',
            });

            if (!response.ok) {
                throw new Error('Failed to fetch the file');
            }

            // Convert the response to a blob and create a file object
            const blob = await response.blob();
            const newFile = new File([blob], file.fileName, { type: 'text/csv' });

            // Append the file to FormData
            formData.append('bulkFile', newFile);

            // Call the CountBulkStud endpoint to extract valid student IDs
            const countResponse = await fetch(`https://localhost:3003/api/Student/BulkCount`, {
                method: 'POST',
                body: formData,
            });

            if (!countResponse.ok) {
                const errorData = await countResponse.json();
                throw new Error(errorData.message || 'Failed to count student IDs');
            }

            const countResult = await countResponse.json();
            const studentCheckID = countResult.studentCheckID;

            if (!studentCheckID || studentCheckID.length === 0) {
                throw new Error('No valid student IDs found');
            }

            // Add the student ID list to the FormData
            // formData.append('studentCheckID', JSON.stringify(studentCheckID));
            // Append the student IDs individually to FormData
            studentCheckID.forEach(id => {
                formData.append('StudentIDList', id);
            });


            // Call the bulkCheck endpoint with the file and student IDs
            const checkResponse = await fetch(`https://localhost:3003/api/Student/bulkCheck`, {
                method: 'POST',
                body: formData,
            });

            if (!checkResponse.ok) {
                throw new Error('Recheck failed');
            }

            // Handle the response from the backend
            const result = await checkResponse.json();

            // Redirect to the results page with the results as URL parameters
            const encodedResults = encodeURIComponent(JSON.stringify(result.studentSurveyResults));
            window.location.href = `/BulkCheckResult?result=${encodedResults}`;

        } catch (error) {
            alert('Error during recheck: ' + error.message);
        }
    };



    return (
        <div className={styles.pageContainer}>
            <Header />

            {/* Title */}
            <div className={styles.titleContainer}>
                <h1 className={styles.title}>Bulk Check History</h1>
            </div>

            {/* Table */}
            <div className={styles.tableContainer}>
                {loading && <p className={styles.loadingMessage}>Loading...</p>}
                {error && <p className={styles.errorMessage}>No data found: {error}</p>}
                {!loading && !error && (
                    <table className={styles.table}>
                        <thead>
                            <tr>
                                <th>Date of Upload</th>
                                <th>File Name</th>
                                <th>Download .csv</th>
                                <th>Recheck</th>
                            </tr>
                        </thead>
                        <tbody>
                            {bulkData.length > 0 ? (
                                bulkData.map((file, index) => (
                                    <tr key={index}>
                                        <td>{new Date(file.lastModified).toLocaleDateString()}</td>
                                        <td>{file.fileName}</td>
                                        <td>
                                            <button className={`${styles.actionButton} ${styles.downloadButton}`}
                                                onClick={() => download(file)}
                                            >
                                                &#11123; {/* Download icon */}
                                            </button>
                                        </td>
                                        <td>
                                            <button
                                                className={`${styles.actionButton} ${styles.recheckButton}`}
                                                onClick={() => handleRecheck(file)} // Call handleRecheck on click
                                            >
                                                &#x21bb; {/* Recheck icon */}
                                            </button>
                                        </td>
                                    </tr>
                                ))
                            ) : (
                                    <tr>
                                        <td colSpan="4" className="no-data-message">
                                            No csv Files found
                                        </td>
                                    </tr>
                            )}
                        </tbody>
                    </table>
                )}
            </div>

            <Image src={Bttm} className={styles.bottomImage} alt="Bottom Image" />
        </div>
    );
}

export default BulkCheckHistory;
