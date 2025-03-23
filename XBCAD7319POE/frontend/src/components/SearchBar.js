import React, { useState, useEffect } from 'react';
import styles from './SearchBar.module.css'; // Import the CSS module

const SearchBar = ({ onSearch, onClear, presetValue}) => {
    const [query, setQuery] = useState('');

    const handleChange = (event) => {
        setQuery(event.target.value);
    };

    const handleSubmit = (event) => {
        event.preventDefault(); // Prevent the form from submitting
        onSearch(query); // Trigger search with the current query
    };

    const handleClear = () => {
        setQuery(''); // Reset input field
        if (onClear) {
            onClear(); // Notify parent to clear the filtered results
        }
    };

    useEffect(() => {
        // Set the search value to the preset value when the component mounts
        setQuery(presetValue);
    }, [presetValue]);

    return (
        <form className={styles.searchBar} onSubmit={handleSubmit}>
            <input
                type="text"
                value={query}
                onChange={handleChange}
                placeholder="Enter Student ID"
                className={styles.input}
            />
            <button type="submit" className={styles.button}>Search</button>
            <button type="button" className={styles.clearButton} onClick={handleClear}>Clear</button>
        </form>
    );
};

export default SearchBar;
