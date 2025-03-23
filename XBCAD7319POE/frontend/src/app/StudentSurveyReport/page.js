"use client";
import { useState, useEffect } from 'react';
import Header from '@/components/Header';
import Footer from '@/components/Footer';
import styles from './StudentSurveyReport.module.css';
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js';
import { Pie } from 'react-chartjs-2';

ChartJS.register(ArcElement, Tooltip, Legend);

const StudentSurveyReport = () => {
    // Get current month and year
    const currentDate = new Date();
    const currentMonth = currentDate.toLocaleString('default', { month: 'long' });
    const currentYear = currentDate.getFullYear();

    const [month, setMonth] = useState(currentMonth);
    const [year, setYear] = useState(currentYear.toString());
    const [data, setData] = useState([]);
    const [completedCount, setCompletedCount] = useState(0);
    const [totalCount, setTotalCount] = useState(0);
    const [instituteCounts, setInstituteCounts] = useState({}); // State for institute counts
    const [popularInst, setPopularInst] = useState([]);

    const fetchData = async () => {
        if (!month || !year) return;

        try {
            const response = await fetch(`https://localhost:3003/api/Student/report?Month=${month}&year=${year}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                },
            });

            if (!response.ok) {
                throw new Error(`Failed to fetch: ${response.statusText}`);
            }

            const responseData = await response.json();
            setData(responseData.students || []); // Adjusted based on new data structure
            setCompletedCount(responseData.numCreatedCompleted || 0);
            setTotalCount(responseData.numCreated || 0); // Set total created completed count
            setInstituteCounts(responseData.instituteCounts || {}); // Set institute counts
            setPopularInst(responseData.popularInst || []);
        } catch (error) {
            setData([]);
            setCompletedCount(0);
            setTotalCount(0);
            setInstituteCounts({});
            console.error('Error fetching survey data:', error.message);
        }
    };

    useEffect(() => {
        fetchData();
    }, [month, year]);

    // Pie chart for completed vs created surveys
    const applicantChartData = {
        labels: ['Completed', 'Created'],
        datasets: [
            {
                data: [completedCount, totalCount],
                backgroundColor: ['#41B4E0', '#092644'],
            },
        ],
    };

    // Pie chart for institute counts
    const instituteChartData = {
        labels: Object.keys(instituteCounts),
        datasets: [
            {
                data: Object.values(instituteCounts),
                backgroundColor: ['#EF6551', '#FEC900', '#BCD631', '#D1E1E0', '#41B4E0', '#58595B', '#092644', '#FFFFFF', '#000000'],
            },
        ],
    };

    const handleSearch = () => {
        fetchData();
        console.log('Search button clicked');
    };

    const handleReset = () => {
        setMonth(currentMonth);
        setYear(currentYear.toString());
        fetchData(); // Fetch data for the current month and year
        console.log('Reset button clicked');
    };

    return (
        <div className={styles.container}>
            <Header />
            <div className={styles.heading}>Student Survey Report</div>

            {/* Full Table Section with Centered Filter */}
            <div className={styles.tableChartContainer}>
                <div className={styles.fullTableSection}>
                    {/* Centered filter section */}
                    <div className={styles.filterContainer}>
                        <label htmlFor="month" className={styles.label}>Month:</label>
                        <select
                            id="month"
                            value={month}
                            onChange={(e) => setMonth(e.target.value)}
                            className={styles.dropdown}
                        >
                            {Array.from({ length: 12 }, (v, i) => (
                                <option key={i} value={new Date(0, i).toLocaleString('default', { month: 'long' })}>
                                    {new Date(0, i).toLocaleString('default', { month: 'long' })}
                                </option>
                            ))}
                        </select>

                        <label htmlFor="year" className={styles.label}>Year:</label>
                        <select
                            id="year"
                            value={year}
                            onChange={(e) => setYear(e.target.value)}
                            className={styles.dropdown}
                        >
                            {Array.from({ length: 7 }, (v, i) => (
                                <option key={i} value={(2022) + i}>{2022 + i}</option>
                            ))}
                        </select>
                    </div>

                    <div className={styles.tableSection}>
                        <table className={styles.table}>
                            <thead>
                                <tr>
                                    <th>Date</th>
                                    <th>Student ID</th>
                                    <th>Status</th>
                                </tr>
                            </thead>
                            <tbody>
                                {data.length === 0 ? (
                                    <tr>
                                        <td colSpan="3">No data available for {month} {year}.</td>
                                    </tr>
                                ) : (
                                    data.map((item, index) => (
                                        <tr key={index} className={index % 2 === 0 ? styles.rowEven : styles.rowOdd}>
                                            <td>{new Date(item.date).toLocaleDateString()}</td>
                                            <td>{item.student_id}</td>
                                            <td>{item.status || 'No'}</td>
                                        </tr>
                                    ))
                                )}
                            </tbody>
                        </table>
                    </div>
                </div>

                <div className={styles.chartSection}>
                    <div className={styles.ChartItself}>
                         <h3>Survey Completion Status</h3>
                        <div className={styles.firstPieChart}>

                            <Pie data={applicantChartData} />
                        </div>
                    </div>
                    <div className={styles.ChartItself}>
                        
                        <div className={styles.secondPieChart}>
                            <h3>Students' Insitutions</h3>
                            <Pie data={instituteChartData} />
                        </div>
                    </div>
                </div>
            </div>
            <Footer />
        </div>
    );

};

export default StudentSurveyReport;
