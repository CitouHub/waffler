import React from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faChartLine, faRulerCombined, faTools } from "@fortawesome/free-solid-svg-icons";
import { NavItem, NavLink, Nav } from "reactstrap";
import classNames from "classnames";
import { Link } from "react-router-dom";

import Balance from "../status/balance";

import './sidebar.css';

const SideBar = ({ isOpen, toggle }) => (
    <div className={classNames("sidebar", { "is-open": isOpen })}>
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
                    <NavLink tag={Link} to={"/traderules"}>
                        <FontAwesomeIcon icon={faRulerCombined} className="mr-2" />
                        Rules
                    </NavLink>
                </NavItem>
                <NavItem>
                    <NavLink tag={Link} to={"/traderules"}>
                        <FontAwesomeIcon icon={faTools} className="mr-2" />
                        Settings
                    </NavLink>
                </NavItem>
            </Nav>
        </div>
    </div>
);

export default SideBar;
