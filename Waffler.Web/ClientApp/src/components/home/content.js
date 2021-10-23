import React, { useLayoutEffect, useRef, useState} from "react";
import classNames from "classnames";
import { Container } from "reactstrap";
import { Switch, Route } from "react-router-dom";

import TradeChart from "../chart/tradechart";

const Content = ({ sidebarIsOpen, toggleSidebar }) => {
    return (
        <Container
            fluid
            className={classNames("content", { "is-open": sidebarIsOpen })}
        >
            <Switch>
                <Route exact path="/" component={() => <TradeChart type="svg" />} />
                <Route exact path="/traderules" component={() => "Trade rules"} />
            </Switch>
        </Container>
    );
}
export default Content;