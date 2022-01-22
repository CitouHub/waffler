import React from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faChartLine, faRulerCombined, faTools, faSignOutAlt, faTasks, faChartPie } from "@fortawesome/free-solid-svg-icons";
import { NavItem, NavLink, Nav } from "reactstrap";
import { Link } from "react-router-dom";

import Balance from "../../navigation/status/balance";
import SystemStatus from "../../navigation/status/system.status";

import './sidebar.css';

const SideBar = () => {

    const routeActive = (route) => {
        return window.location.href.endsWith(route);
    }

    return (
        <div className="sidebar is-open">
            <div className="sidebar-header">
                <Balance />
                <SystemStatus />
            </div>
            <div className="side-menu">
                <Nav vertical className="list-unstyled pb-3">
                    <NavItem>
                        <NavLink tag={Link} to={"/tradechart"} className={routeActive("/tradechart") ? 'route-active' : ''}>
                            <FontAwesomeIcon icon={faChartLine} className="mr-2" />
                            Chart
                        </NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink tag={Link} to={"/traderules"} className={routeActive("/traderules") ? 'route-active' : ''}>
                            <FontAwesomeIcon icon={faRulerCombined} className="mr-2" />
                            Rules
                        </NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink tag={Link} to={"/statitics/buy"} className={routeActive("/statitics/buy") ? 'route-active' : ''}>
                            <FontAwesomeIcon icon={faChartPie} className="mr-2" />
                            Statistics
                        </NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink tag={Link} to={"/tradeorders"} className={routeActive("/tradeorders") ? 'route-active' : ''}>
                            <FontAwesomeIcon icon={faTasks} className="mr-2" />
                            Orders
                        </NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink tag={Link} to={"/profile"} className={routeActive("/profile") ? 'route-active' : ''}>
                            <FontAwesomeIcon icon={faTools} className="mr-2" />
                            Profile
                        </NavLink>
                    </NavItem>

                    <NavItem className="logout">
                        <NavLink tag={Link} to={"/logout"}>
                            <FontAwesomeIcon icon={faSignOutAlt} className="mr-2" />
                            Log out
                        </NavLink>
                    </NavItem>
                </Nav>
            </div>
        </div>
    );
}

export default SideBar;
