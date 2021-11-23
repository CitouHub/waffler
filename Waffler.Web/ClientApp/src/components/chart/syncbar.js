import React from 'react';

import './chart.css';

const SyncBar = ({ progress, currentDate }) => {
    return (
        <div className="sync-bar mt-3 mb-3">
            <h4>Syncing data, please wait...</h4>
            {progress >= 1 && <div className="sync-progress" style={{ width: `${progress}%` }}>
                {currentDate && <span>{currentDate}</span>}
            </div>}
        </div>
    )
};

export default SyncBar;
