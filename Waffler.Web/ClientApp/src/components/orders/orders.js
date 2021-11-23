import React, { useState, useEffect } from "react";
import Button from '@mui/material/Button';

import LoadingBar from '../../components/utils/loadingbar';

const Orders = () => {
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        console.log('Get orders');
        setLoading(false);
    }, []);

    return (
        <div>
            <LoadingBar active={loading} />
            <div className='mt-3 mb-3 control'>
                <h4>Orders</h4>
            </div>
        </div>
    )
};

export default Orders;
