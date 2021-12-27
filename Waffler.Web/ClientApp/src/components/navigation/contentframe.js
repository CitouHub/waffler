import React from 'react';
import { Container } from "reactstrap";
import Cache from '../../util/cache';

import TopBar from './topbar/topbar';
import SideBar from './sidebar/sidebar';

import './contentframe.css';

const ContentFrame = (props) => {
    const isAuthenticated = Cache.get("isAuthenticated");

    if (isAuthenticated === true) {
        return (
            <div className="home wrapper">
                <TopBar />
                <div className="page">
                    <SideBar />
                    <Container fluid className="content">
                        {props.children}
                    </Container>
                </div>
            </div>
        );
    } else {
        return (
            <div>
                {props.children}
            </div>
        )
    }
}

export default ContentFrame;