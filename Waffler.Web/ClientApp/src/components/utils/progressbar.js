import React from 'react';

import './progressbar.css';

const ProgressBar = ({ progress }) => {
    return (
        <div className="progress-line" style={{ width: `${progress}%` }} />
    )
};

export default ProgressBar;
