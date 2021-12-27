﻿import React from "react";
import { Redirect, Route } from "react-router-dom";

import Cache from '../../util/cache'

function ProtectedRoute({ component: Component, ...restOfProps }) {
    const isAuthenticated = Cache.get("isAuthenticated");

    if (isAuthenticated === true) {
        Cache.set("isAuthenticated", true);
    }

    return (
        <Route
            {...restOfProps}
            render={(props) =>
                isAuthenticated ? <Component {...props} /> : <Redirect to="/login" />
            }
        />
    );
}

export default ProtectedRoute;