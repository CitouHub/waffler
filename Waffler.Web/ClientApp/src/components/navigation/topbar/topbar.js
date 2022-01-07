import React, { useState, useEffect } from 'react';
import Config from '../../../util/config';
import ProfileService from '../../../services/profile.service';

import waffle from '../../../../src/assets/images/waffle.png';
import './topbar.css';

const TopBar = () => {
    const [loading, setLoading] = useState(true);
    const [latestRelease, setLatestRelease] = useState('');

    useEffect(() => {
        ProfileService.getLatestRelease().then((release) => {
            setLatestRelease(release);
            setLoading(false);
        });
    }, []);

    return (
        <div className="topbar">
            <img src={waffle} alt="Logo" className="waffle-small" />
            <div className="d-flex">
                <div className="title">
                    {loading === false && latestRelease !== Config.version() && <p>{latestRelease}<br />available</p>}
                </div>
                <div className="title">
                    <h4>waffler<br /><span className="version">{Config.version()}</span></h4>
                </div>
            </div>
        </div>
    );
}

export default TopBar;
