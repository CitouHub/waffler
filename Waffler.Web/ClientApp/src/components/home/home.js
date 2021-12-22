import React, { useState, useEffect } from 'react';
import { Redirect } from "react-router-dom";

import Cache from '../../util/cache'
import ProfileService from '../../services/profile.service'

const Home = () => {
    const [loading, setLoading] = useState(true);
    const [hasProfile, setHasProfile] = useState();

    const isAuthenticated = Cache.get("isAuthenticated");

    useEffect(() => {
        ProfileService.hasProfile().then((value) => {
            setHasProfile(value);
            setLoading(false);
        });
    }, []);

    if (!loading) {
        if (hasProfile && isAuthenticated) {
            return (
                <Redirect to="/tradechart" />
            );
        } else {
            if (!hasProfile) {
                return <Redirect to="/newprofile" />
            } else if (!isAuthenticated) {
                return <Redirect to="/login" />
            }
        }
    } else {
        return null;
    }
}

export default Home;