import React from "react";
import classNames from "classnames";
import { Container } from "reactstrap";
import { Switch } from "react-router-dom";

import Cache from '../../util/cache'
import ProtectedRount from '../home/protectedroute'
import TradeChart from "../chart/tradechart";
import TradeRules from "../traderules/traderules";

const Content = ({ sidebarIsOpen, toggleSidebar }) => {
    return (
        <Container
            fluid
            className={classNames("content", { "is-open": sidebarIsOpen })}
        >
            <Switch>
                <ProtectedRount exact path="/c" component={() => <TradeChart />} />
                <ProtectedRount exact path="/" component={() => <TradeRules />} />
                <ProtectedRount exact path="/settings" component={() => "Settings"} />
                <ProtectedRount exact path="/logout" component={() => {
                    Cache.set("isAuthenticated", false);
                    window.location.replace("/login");
                }} />
            </Switch>
        </Container>
    );
}
export default Content;