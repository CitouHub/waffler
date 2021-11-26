import React from "react";
import classNames from "classnames";
import { Container } from "reactstrap";
import { Switch } from "react-router-dom";

import Cache from '../../util/cache'
import ProtectedRount from '../home/protectedroute'
import TradeChart from "../chart/tradechart";
import TradeRules from "../traderules/traderules";
import Profile from "../profile/profile";
import Orders from "../orders/orders";

const Content = ({ sidebarIsOpen }) => {
    return (
        <Container fluid className={classNames("content", { "is-open": sidebarIsOpen })}>
            <Switch>
                <ProtectedRount exact path="/" component={() => <TradeChart />} />
                <ProtectedRount exact path="/tradeorders" component={() => <Orders />} />
                <ProtectedRount exact path="/traderules" component={() => <TradeRules />} />
                <ProtectedRount exact path="/profile" component={() => <Profile />} />
                <ProtectedRount exact path="/logout" component={() => {
                    Cache.set("isAuthenticated", false);
                    window.location.replace("/login");
                }} />
            </Switch>
        </Container>
    );
}
export default Content;