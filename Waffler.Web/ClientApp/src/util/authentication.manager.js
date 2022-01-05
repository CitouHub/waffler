import React, { useEffect } from 'react';
import { useHistory } from 'react-router-dom';
import Cache from './cache';

const AuthenticationManager = (props) => {
    let history = useHistory();

    useEffect(() => {
        checkAuthentication();
    }, []);

    const checkAuthentication = () => {
        console.log("Check auth!");
        console.log(window.location.href);
        if (!window.location.href.endsWith("/login") && !window.location.href.endsWith("/newprofile") && !window.location.href.endsWith("/")) {
            const isAuthenticated = Cache.get("isAuthenticated");
            if (isAuthenticated !== true) {
                console.log("Kickout!");
                history.push("/");
            } else {
                setTimeout(() => checkAuthentication(), 5000);
            }
        }
    }

    return (
        <React.Fragment>
            {props.children}
        </React.Fragment>
    );
}

export default AuthenticationManager;