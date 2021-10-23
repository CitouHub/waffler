import React from "react";

import waffle from '../../../../../src/assets/images/waffle.png';
import './topbar.css';

const TopBar = () => (
    <div className="topbar">
        <img src={waffle} alt="Logo" className="waffle" />
        <div className="title">
            <h4>waffler</h4>
        </div>
    </div>
);

export default TopBar;
