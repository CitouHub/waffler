import React from 'react';

import './loadingbar.css';

const LoadingBar = (props) => {
    if (props.active || props.active === undefined) {
        return (
            <div className="loading-line"></div>
        )
    } else {
        return (
            <div className="loading-placeholder"></div>
        )
    }
};

export default LoadingBar;
