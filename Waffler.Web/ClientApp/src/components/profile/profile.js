import React, { useState, useEffect } from "react";

import LoadingBar from '../../components/utils/loadingbar';

const Profile = () => {
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        console.log('Get profile');
        setLoading(false);
    }, []);

    return (
        <div>
            <LoadingBar active={loading} />
            <div className='mt-3 mb-3'>
                <h4>Profile</h4>
            </div>
        </div>
    )
};

export default Profile;
