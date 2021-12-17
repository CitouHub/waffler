import React, { useState, useEffect } from 'react';
import { BrowserRouter, Route, Switch } from 'react-router-dom';
import "bootstrap/dist/css/bootstrap.min.css";
import Config from './util/config'

import AppSettingsService from './services/appsettings.service'

import Login from './components/login/login'
import NewProfile from './components/login/newprofile'
import Home from './components/home/home'

import './app.css';

const App = () => {
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        AppSettingsService.get().then((value) => {
            Config.setApplicationSettings(value);
            setLoading(false);
        });
    }, []);

    if (!loading) {
        return (
            <BrowserRouter>
                <Switch>
                    <Route path="/" exact component={Home} />
                    <Route path="/login" exact component={Login} />
                    <Route path="/newprofile" exact component={NewProfile} />
                </Switch>
            </BrowserRouter>
        );
    } else {
        return null;
    }
};

export default App;
