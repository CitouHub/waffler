import React from 'react';

import './chart.css';

const SyncBar = ({ progress, currentDate, throttled}) => {
    return (
        <div className="sync-bar mt-3 mb-3">
            <h4>Syncing data, please wait...</h4>
            {currentDate && <span>Now fetching data for {currentDate} <strong>{Math.round(progress)}%</strong>{throttled ? ' (Throttle...)' : ''}</span>}
            {progress >= 1 && <div className="sync-progress" style={{ width: `${progress}%` }}>
            </div>}
        </div>
    )
};

export default SyncBar;
