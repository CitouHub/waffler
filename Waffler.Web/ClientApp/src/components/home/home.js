import React, { useState, useEffect } from 'react';
import { BrowserRouter, Redirect } from "react-router-dom";

import Cache from '../../util/cache'
import TopBar from './navigation/topbar/topbar';
import SideBar from './navigation/sidebar/sidebar';
import Content from './content';

import ProfileService from '../../services/profile.service'

import './home.css';

const Home = () => {
    const [loading, setLoading] = useState(true);
    const [hasProfile, setHasProfile] = useState();
    const [sidebarIsOpen, setSidebarOpen] = useState(true);
    const toggleSidebar = () => setSidebarOpen(!sidebarIsOpen);

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
                <BrowserRouter>
                    <div className="home wrapper">
                        <TopBar />
                        <div className="page">
                            <SideBar toggle={toggleSidebar} isOpen={sidebarIsOpen} />
                            <Content toggleSidebar={toggleSidebar} sidebarIsOpen={sidebarIsOpen} />
                        </div>
                    </div>
                </BrowserRouter>
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