import React from 'react';
import { Container } from "reactstrap";

import TopBar from './topbar/topbar';
import SideBar from './sidebar/sidebar';

import './contentframe.css';

const ContentFrame = (props) => {
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
}

export default ContentFrame;