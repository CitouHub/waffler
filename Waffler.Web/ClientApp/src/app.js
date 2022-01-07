import React, { useState, useEffect } from 'react';
import { BrowserRouter, Route, Switch, Redirect } from 'react-router-dom';
import Config from './util/config';
import Cache from './util/cache';

import AppSettingsService from './services/appsettings.service';

import ProtectedRount from './components/navigation/protectedroute';
import ContentFrame from './components/navigation/contentframe';
import Home from './components/home/home';
import NewProfile from './components/login/newprofile';
import Login from './components/login/login';
import TradeChart from './components/chart/tradechart';
import TradeRules from './components/traderules/traderules';
import TradeOrders from './components/tradeorders/tradeorders';
import Profile from './components/profile/profile';
import TradeRuleBuyStatistics from './components/statistics/traderule/buy/traderule.statistics.buy';

import "bootstrap/dist/css/bootstrap.min.css";
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
                    <ContentFrame>
                        <Route path="/" exact component={Home} />

                        <ProtectedRount path="/tradechart" exact component={TradeChart} />
                        <ProtectedRount path="/traderules" exact component={TradeRules} />
                        <ProtectedRount path="/tradeorders" exact component={TradeOrders} />
                        <ProtectedRount path="/profile" exact component={Profile} />
                        <ProtectedRount path="/statitics/buy" exact component={TradeRuleBuyStatistics} />

                        <Route path="/login" exact component={Login} />
                        <Route path="/newprofile" exact component={NewProfile} />

                        <Route exact path="/logout" component={() => {
                            Cache.set("isAuthenticated", false);
                            return <Redirect to="/login" />
                        }} />
                    </ContentFrame>
                </Switch>
            </BrowserRouter>
        );
    } else {
        return null;
    }
};

export default App;
