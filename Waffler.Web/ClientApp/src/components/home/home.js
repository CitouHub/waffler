import React, { useState, useEffect } from 'react';
import { Redirect } from "react-router-dom";
import CircularProgress from '@mui/material/CircularProgress';
import Box from '@mui/material/Box';

import Cache from '../../util/cache';
import ProfileService from '../../services/profile.service';
import StatusService from '../../services/status.service';

import waffle from '../../assets/images/waffle.png';

const Home = () => {
    const [loading, setLoading] = useState(true);
    const [hasProfile, setHasProfile] = useState(false);

    const isAuthenticated = Cache.getAndReset("isAuthenticated");

    useEffect(() => {
        StatusService.awaitDatabaseOnline().then(() => {
            if (!isAuthenticated) {
                ProfileService.hasProfile().then((value) => {
                    setHasProfile(value);
                    setLoading(false);
                });
            } else {
                setLoading(false);
            }
        });
    }, []);

    if (!loading) {
        if (isAuthenticated) {
            return <Redirect to="/tradechart" />
        } else if (hasProfile && !isAuthenticated) {
            return <Redirect to="/login" />
        } else {
            return <Redirect to="/newprofile" />
        }
    } else {
        if (!isAuthenticated) {
            return (
                <div className="p-center">
                    <img src={waffle} alt="Logo" className="waffle mb-3" />
                    <Box sx={{ display: 'flex' }}>
                        <CircularProgress />
                    </Box>
                </div>
            )
        } else if (isAuthenticated) {
            return null;
        }
    }
}

export default Home;