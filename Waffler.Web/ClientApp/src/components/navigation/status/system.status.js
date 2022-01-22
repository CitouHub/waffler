import React, { useEffect } from 'react';

import StatusService from '../../../services/status.service'
import Cache from '../../../util/cache';

let unmount = false;

const SystemStatus = () => {
    useEffect(() => {
        unmount = false;
        checkDatabaseOnline();
        return () => {
            unmount = true;
        }
    }, []);

    const checkDatabaseOnline = () => {
        StatusService.isDatabaseOnline().then((online) => {
            if (unmount === false) {
                if (online === false) {
                    Cache.set("isAuthenticated", false);
                } else {
                    setTimeout(() => checkDatabaseOnline(), 5000);
                }
            }
        });
    }

    return (<div></div>);
}
export default SystemStatus;
