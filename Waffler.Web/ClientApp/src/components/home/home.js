import React, { useState } from 'react';
import { BrowserRouter, Redirect } from "react-router-dom";

import Cache from '../../util/cache'
import TopBar from './navigation/topbar/topbar';
import SideBar from './navigation/sidebar/sidebar';
import Content from './content';

import './home.css';

const Home = (props) => {
    const [sidebarIsOpen, setSidebarOpen] = useState(true);
    const toggleSidebar = () => setSidebarOpen(!sidebarIsOpen);

    const isAuthenticated = Cache.get("isAuthenticated");

    if (props.hasProfile && isAuthenticated) {
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
        if (!props.hasProfile) {
            return <Redirect to="/newprofile" />
        } else if (!isAuthenticated) {
            return <Redirect to="/login" />
        }
    }
}

export default Home;