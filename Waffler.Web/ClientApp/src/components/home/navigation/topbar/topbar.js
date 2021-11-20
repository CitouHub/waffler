import React from "react";

import waffle from '../../../../../src/assets/images/waffle.png';
import './topbar.css';

const TopBar = () => (
    <div className="topbar">
        <img src={waffle} alt="Logo" className="waffle" />
        <div className="title">
            <h4>waffler<br /><span className="version">v0.1.0-beta</span></h4>
        </div>
    </div>
);

export default TopBar;
