import React, { useState, useEffect } from 'react';
import { BrowserRouter, Route, Switch } from 'react-router-dom';
import "bootstrap/dist/css/bootstrap.min.css";
import Config from './util/config'

import AppSettingsService from './services/appsettings.service'
import ProfileService from './services/profile.service'

import Login from './components/login/login'
import NewProfile from './components/login/newprofile'
import Home from './components/home/home'

import './App.css';

const App = () => {
    const [loading, setLoading] = useState(true);
    const [hasProfile, setHasProfile] = useState();

    useEffect(() => {
        AppSettingsService.get().then((value) => {
            Config.setApplicationSettings(value);
            ProfileService.hasProfile().then((value) => {
                setHasProfile(value);
            });
        });
    }, []);

    useEffect(() => {
        if (hasProfile !== undefined) {
            setLoading(false);
        }
    }, [hasProfile]);

    return (
        <BrowserRouter>
            <Switch>
                <Route path="/" exact render={(props) => {
                    if (!loading) {
                        return (<Home {...props} hasProfile={hasProfile} />)
                    }
                }} />
                <Route path="/login" exact component={Login} />
                <Route path="/newprofile" exact component={NewProfile} />
            </Switch>
        </BrowserRouter>
    );
};

export default App;