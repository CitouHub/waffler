import React from "react";
import Config from '../../../util/config';
import waffle from '../../../../src/assets/images/waffle.png';

import './topbar.css';

const TopBar = () => (
    <div className="topbar">
        <img src={waffle} alt="Logo" className="waffle-small" />
        <div className="title">
            <h4>waffler<br /><span className="version">{Config.version()}</span></h4>
        </div>
    </div>
);

export default TopBar;
