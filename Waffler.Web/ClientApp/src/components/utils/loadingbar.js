import React from 'react';

import './loadingbar.css';

const LoadingBar = (props) => {
    if (props.active || props.active === undefined) {
        return (
            <div className="progress-line"></div>
        )
    } else {
        return null;
    }
};

export default LoadingBar;
