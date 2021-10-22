import React, { useState } from 'react';
import { BrowserRouter, Redirect } from "react-router-dom";

import SideBar from './sidebar/sidebar';
import Content from './content';

const Home = (props) => {
    const [sidebarIsOpen, setSidebarOpen] = useState(true);
    const toggleSidebar = () => setSidebarOpen(!sidebarIsOpen);

    const isAuthenticated = localStorage.getItem("isAuthenticated");

    if (props.hasProfile && isAuthenticated) {
        return (
            <BrowserRouter>
                <div className="home wrapper">
                    <SideBar toggle={toggleSidebar} isOpen={sidebarIsOpen} />
                    <Content toggleSidebar={toggleSidebar} sidebarIsOpen={sidebarIsOpen} />
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