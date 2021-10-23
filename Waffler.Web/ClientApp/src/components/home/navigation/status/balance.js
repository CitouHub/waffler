import React, { useState, useEffect } from 'react';
import LoadingBar from '../../../utils/loadingbar'

import ProfileService from '../../../../services/profile.service'

import './status.css';

const Balance = () => {
    const [loading, setLoading] = useState(true);
    const [balance, setBalance] = useState([]);

    useEffect(() => {
        ProfileService.getBalance().then((value) => {
            setBalance(value);
            setLoading(false);
        });
    }, []);

    let btc = balance.find(({ currencyCode }) => currencyCode === 1);
    let eur = balance.find(({ currencyCode }) => currencyCode === 2);
    return (
        <div className="balance">
            <LoadingBar active={loading} />
            <ul>
                <li>Balance</li>
                <li>BTC: {btc?.available ?? '-'} / {btc?.tradeLocked ?? '-'}</li>
                <li>EUR: {eur?.available ?? '-'} / {eur?.tradeLocked ?? '-'}</li>
            </ul>
        </div>
    );
}
export default Balance;
