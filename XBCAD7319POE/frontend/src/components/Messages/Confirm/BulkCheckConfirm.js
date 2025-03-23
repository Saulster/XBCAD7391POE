// BulkCheckConfirm.js
import React from 'react';
import styles from './BulkCheckConfirm.module.css';
import Image from 'next/image';
import bellIcon from '@/public/bell.png';

const BulkCheckConfirm = ({ count, onProceed, onCancel }) => {
    return (
        <div className={styles.container}>
            <div className={styles.confirmBox}>
                <p className={styles.message}>
                    <span>&#x1F514; </span>You are about to perform <strong>{count}</strong> Survey Completion checks.<br /> Do you wish to continue?
                </p>
            </div>
            <div className={styles.buttonsContainer}>
                <button className={styles.button} onClick={onProceed}>Continue</button>
                <button className={styles.cancelButton} onClick={onCancel}>Cancel</button>
            </div>
        </div>
    );
};

export default BulkCheckConfirm;
