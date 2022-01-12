import React, { useState, useEffect } from 'react';
import LoadingBar from '../../../components/utils/loadingbar'

import ProfileService from '../../../services/profile.service'

import './status.css';

let unmount = false;

const Balance = () => {
    const [loading, setLoading] = useState(true);
    const [balance, setBalance] = useState([]);

    useEffect(() => {
        unmount = false;
        updateBalance();
        return () => {
            unmount = true;
        }
    }, []);

    const updateBalance = () => {
        ProfileService.getBalance().then((value) => {
            if (unmount === false) {
                setBalance(value);
                setLoading(false);

                setTimeout(() => updateBalance(), 10000);
            }
        });
    }

    let btc = balance.find(({ currencyCode }) => currencyCode === 1);
    let eur = balance.find(({ currencyCode }) => currencyCode === 2);
    return (
        <div className="balance">
            <LoadingBar active={loading} />
            <ul>
                <li>Balance</li>
                <li>{btc?.available ?? '-'} ₿</li>
                <li>{eur?.available ?? '-'} €</li>
            </ul>
        </div>
    );
}
export default Balance;
