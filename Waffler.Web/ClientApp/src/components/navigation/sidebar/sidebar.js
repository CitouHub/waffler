import React from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faChartLine, faRulerCombined, faTools, faSignOutAlt, faTasks } from "@fortawesome/free-solid-svg-icons";
import { NavItem, NavLink, Nav } from "reactstrap";
import { Link } from "react-router-dom";

import Balance from "../../navigation/status/balance";

import './sidebar.css';

const SideBar = () => (
    <div className="sidebar is-open">
        <div className="sidebar-header">
            <Balance/>
        </div>
        <div className="side-menu">
            <Nav vertical className="list-unstyled pb-3">
                <NavItem>
                    <NavLink tag={Link} to={"/"}>
                        <FontAwesomeIcon icon={faChartLine} className="mr-2" />
                        Chart
                    </NavLink>
                </NavItem>
                <NavItem>
                    <NavLink tag={Link} to={"/tradeorders"}>
                        <FontAwesomeIcon icon={faTasks} className="mr-2" />
                        Orders
                    </NavLink>
                </NavItem>
                <NavItem>
                    <NavLink tag={Link} to={"/traderules"}>
                        <FontAwesomeIcon icon={faRulerCombined} className="mr-2" />
                        Rules
                    </NavLink>
                </NavItem>
                <NavItem>
                    <NavLink tag={Link} to={"/profile"}>
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

export default SideBar;
