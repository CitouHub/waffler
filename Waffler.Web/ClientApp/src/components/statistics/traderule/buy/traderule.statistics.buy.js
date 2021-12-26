﻿import React, { useState, useEffect } from "react";

import LoadingBar from '../../../utils/loadingbar';

const TradeRuleBuyStatistics = () => {
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        setLoading(false);
    }, []);

    return (
        <div>
            <LoadingBar active={loading} />
            <div className='mt-3 mb-3 control'>
                <h4>Statistics</h4>
            </div>
        </div>
    )
};

export default TradeRuleBuyStatistics;