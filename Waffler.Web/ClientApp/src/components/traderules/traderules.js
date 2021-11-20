import React, { useState, useEffect } from "react";
import Button from '@mui/material/Button';

import LoadingBar from '../../components/utils/loadingbar';
import TradeRuleTable from './table/traderule.table';

import TradeRuleService from '../../services/traderule.service';

import './traderule.css';

const TradeRules = () => {
    const [loading, setLoading] = useState(true);
    const [tradeRules, setTradeRules] = useState([]);
    const [tradeRuleAttributes, setTradeRuleAttributes] = useState({});

    const updateTradeRules = () => {
        setLoading(true);
        var getTradeRules = TradeRuleService.getTradeRules();
        var getTradeRuleAttributes = TradeRuleService.getTradeRuleAttributes();

        Promise.all([getTradeRules, getTradeRuleAttributes]).then((result) => {
            setTradeRules(result[0]);
            setTradeRuleAttributes(result[1]);
            setLoading(false);
        });
    }

    const newTradeRule = () => {
        setLoading(true);
        TradeRuleService.newTradeRule().then((result) => {
            setTradeRules([...tradeRules, result]);
            setLoading(false);
        });
    }

    useEffect(() => {
        updateTradeRules();
    }, []);

    return (
        <div>
            <LoadingBar active={loading} />
            <div className='mt-3 mb-3 control'>
                <Button variant="contained" onClick={newTradeRule} disabled={loading}>+ Add new trade rule</Button>
            </div>
            {tradeRules.length > 0 && Object.keys(tradeRuleAttributes).length > 0 &&
                <TradeRuleTable
                    tradeRules={tradeRules}
                    tradeRuleAttributes={tradeRuleAttributes}
                    updateTradeRules={updateTradeRules}
                />
            }
        </div>
    )
};

export default TradeRules;
