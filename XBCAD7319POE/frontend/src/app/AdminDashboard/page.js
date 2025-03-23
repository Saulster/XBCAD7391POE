'use client';

import React, { useEffect, useState } from 'react';
import styles from './AdminDashboardStyle.module.css';
import { useRouter, useSearchParams } from 'next/navigation';
import Image from 'next/image';
import splicetop from '@/public/P2Splice.png';
import splicebottom from '@/public/P3Splice.png';
import Header from '@/components/Header.js';
import SearchBar from '@/components/SearchBar.js';

const AdminDashboard = () => {
    const [students, setStudents] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [filteredStudents, setFilteredStudents] = useState([]);
    const [searchQuery, setSearchQuery] = useState('');
    const searchParams = useSearchParams();
    const sID = searchParams.get('sID');

    const fetchStudents = async (studentId) => {
        setLoading(true);
        try {
            const res = await fetch('https://localhost:3003/api/Student/all', {
                cache: 'no-store',
            });

            if (!res.ok) {
                throw new Error('Failed to fetch students');
            }

            const data = await res.json();
            setStudents(data);

            const filterId = studentId || sID;
            if (filterId) {
                const filtered = data.filter(student =>
                    student.student_id.toLowerCase().includes(filterId.toLowerCase())
                );
                setFilteredStudents(filtered);
                setSearchQuery(filterId); // Set the search bar to the filtered student ID
            } else {
                setFilteredStudents(data);
            }
        } catch (error) {
            console.error('Error fetching students:', error);
            setError(error.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (typeof window !== 'undefined') {
            fetchStudents();
        }
    }, [sID]);

    const handleSearch = (query) => {
        setSearchQuery(query); // Update the search query state
        if (query) {
            const filtered = students.filter(student =>
                student.student_id.toLowerCase().includes(query.toLowerCase())
            );
            setFilteredStudents(filtered);
        } else {
            setFilteredStudents(students);
        }
    };

    const handleClear = () => {
        setSearchQuery(''); // Clear the search query
        setFilteredStudents(students);
    };

    const recheck = async (student) => {
        try {
            const response = await fetch(`https://localhost:3003/api/Student/singleCheck/${student.student_id}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                },
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const data = await response.json();
            alert('Recheck successful');

            // Re-fetch and filter the list to show only the rechecked student
            fetchStudents(student.student_id);
        } catch (error) {
            alert('Error during recheck: ' + error.message);
        }
    };

    return (
        <div>
            <Header />
            <div className={styles.top}>
                <div className={styles.title}>Dashboard</div>
                <Image src={splicetop} className={styles.spliceTop} alt="Top Banner" />
            </div>
            <div className={styles.main}>
                <div className={styles.heading}>Survey Completion Status Table</div>
                <div className={styles.searchContainer}>
                    <SearchBar onSearch={handleSearch} onClear={handleClear} presetValue={searchQuery} />
                </div>
                <div className={styles.tableContainer}>
                    {loading && <p>Loading...</p>}
                    {error && <p>No students found: {error}</p>}
                    {!loading && !error && (
                        <table className={styles.table}>
                            <thead>
                                <tr>
                                    <th>Student ID</th>
                                    <th>Survey Completed</th>
                                    <th>Last Checked</th>
                                    <th>Recheck</th>
                                </tr>
                            </thead>
                            <tbody>
                                {filteredStudents.length > 0 ? (
                                    filteredStudents.map(student => (
                                        <tr key={student.student_id}>
                                            <td>{student.student_id}</td>
                                            <td>{student.status}</td>
                                            <td>{new Date(student.date).toLocaleDateString()}</td>
                                            <td>
                                                <button
                                                    className="refresh-button"
                                                    onClick={() => recheck(student)}
                                                    aria-label={`Recheck ${student.student_id}`}
                                                >
                                                    &#x21bb;
                                                </button>
                                            </td>
                                        </tr>
                                    ))
                                ) : (
                                    <tr>
                                        <td colSpan="4" className="no-data-message">
                                            No students found
                                        </td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    )}
                </div>
            </div>
            <Image src={splicebottom} className={styles.spliceBottom} alt="Bottom Banner" />
        </div>
    );
};

export default AdminDashboard;
