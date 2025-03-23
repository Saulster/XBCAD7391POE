"use client";
import { useState } from "react";
import styles from "./BulkStudentSurveyCheck.module.css"; // CSS Module
import Header from '@/components/Header';
import Footer from '@/components/Footer';
import { useRouter } from 'next/navigation';
import BulkCheckConfirm from '@/components/Messages/Confirm/BulkCheckConfirm'; // Import the component

const BSSC = () => {
    const [file, setFile] = useState(null);
    const [fileName, setFileName] = useState('');  // Track the selected file's name
    const [result, setResult] = useState([]);
    const [count, setCount] = useState(0);
    const [showConfirm, setShowConfirm] = useState(false);
    const [IDList, setIDList] = useState([]);
    const [errorMessage, setErrorMessage] = useState('');  // State to store error messages
    const router = useRouter();

    // Handle file input change
    const handleFileChange = (e) => {
        const selectedFile = e.target.files[0];

        // Validate the file type
        if (!selectedFile || selectedFile.type !== "text/csv") {
            alert("Please select a valid CSV file.");
            e.target.value = ""; // Reset file input if invalid
            setFile(null);
            setFileName(''); // Clear the file name if invalid
            return;
        }

        setFile(selectedFile);
        setFileName(selectedFile.name);  // Store the valid file name
    };

    // Handle file upload cancel
    const handleCancel = () => {
        setShowConfirm(false); // Hide the confirmation box
        setCount(0);
        setIDList([]);
        setFile(null);
        setFileName('');  // Reset file name
        setErrorMessage('');  // Clear any existing error message
        const fileInput = document.querySelector('input[type="file"]');
        if (fileInput) {
            fileInput.value = "";
        }
    };

    // Handle bulk count check
    const handleBulkCount = async (event) => {
        event.preventDefault();

        if (!file) {
            setErrorMessage('Please select a file before uploading.');
            return;
        }

        const formData = new FormData();
        formData.append('bulkFile', file);

        try {
            const response = await fetch(`https://localhost:3003/api/Student/BulkCount`, {
                method: 'POST',
                body: formData,
            });

            const data = await response.json();

            if (!response.ok) {
                setErrorMessage(data.message || 'An error occurred.');
                return;
            }

            setCount(data.counter);
            setIDList(data.studentCheckID);
            setErrorMessage('');  // Clear any error messages on success
            setShowConfirm(true);

        } catch (error) {
            setErrorMessage('Error sending data: ' + error.message);
        }
    };

    // Handle the actual bulk check submission
    const handleBulkCheck = async (event) => {
        event.preventDefault();

        const formData = new FormData();
        formData.append('bulkFile', file); // Append the file to FormData
        formData.append('StudentIDList', JSON.stringify(IDList)); // Append the list of student IDs

        try {
            const response = await fetch(`https://localhost:3003/api/Student/bulkCheck`, {
                method: 'POST',
                body: formData,
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const data = await response.json();
            setResult(data.studentSurveyResults);
            setFile(null); // Clear file input after successful submission
            setErrorMessage('');  // Clear any error messages on success

            // Redirect to BulkCheckResult page after successful check
            const resultString = JSON.stringify(data.studentSurveyResults);
            router.push(`/BulkCheckResult?result=${encodeURIComponent(resultString)}`);
        } catch (error) {
            setErrorMessage('Error sending data: ' + error.message);
        }
    };

    return (
        <div className={styles.container}>
            <Header />
            <div className={styles.main}>
                <div className={styles.backgroundImage}>
                    <div className={styles.headingContainer}>
                        <div className={styles.title}>Bulk Student Survey Check</div>
                        <div className={styles.subtitle}>
                            You are about to perform a bulk check. Make sure that your file meets the following format:
                        </div>
                        <ul className={styles.instructions}>
                            <li>It is a CSV file</li>
                            <li>The column name is <b>student ID</b></li>
                        </ul>
                    </div>
                </div>
            </div>

            <div className={styles.uploadSection}>
                <div className={styles.csvHeading}>Upload CSV file*</div>
                <div className={styles.fileInputWrapper}>
                    <input
                        type="file"
                        accept=".csv"
                        onChange={handleFileChange}
                        className={styles.fileInput}
                    />
                </div>
                {errorMessage && <div className={styles.errorMessage}>{errorMessage}</div>}  {/* Display error messages */}

                {showConfirm && (
                    <BulkCheckConfirm
                        count={count}
                        onProceed={handleBulkCheck}
                        onCancel={handleCancel}
                    />
                )}

                {!showConfirm && (
                    <button className={styles.nextButton} onClick={handleBulkCount}>Next</button>
                )}
            </div>

            <Footer />
        </div>
    );
};

export default BSSC;
