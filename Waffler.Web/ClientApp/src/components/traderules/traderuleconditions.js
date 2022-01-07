import React, { useState, useEffect } from "react";
import Button from '@mui/material/Button';

import LoadingBar from '../../components/utils/loadingbar';
import TradeRuleConditionTable from './table/traderulecondition.table';

import TradeRuleConditionService from '../../services/traderulecondition.service';

import './table.css';

const TradeRuleConditions = ({ tradeRuleId, runningTest }) => {
    const [loading, setLoading] = useState(true);
    const [tradeRuleConditions, setTradeRuleConditions] = useState([]);
    const [tradeRuleConditionAttributes, setTradeRuleConditionAttributes] = useState({});

    const updateTradeRuleConditions = () => {
        setLoading(true);
        var getTradeRuleConditions = TradeRuleConditionService.getTradeRuleConditions(tradeRuleId);
        var getTradeRuleConditionAttributes = TradeRuleConditionService.getTradeRuleConditionAttributes();

        Promise.all([getTradeRuleConditions, getTradeRuleConditionAttributes]).then((result) => {
            setTradeRuleConditions(result[0]);
            setTradeRuleConditionAttributes(result[1]);
            setLoading(false);
        });
    }

    const newTradeRuleCondition = () => {
        setLoading(true);
        TradeRuleConditionService.newTradeRuleCondition(tradeRuleId).then((result) => {
            setTradeRuleConditions([...tradeRuleConditions, result]);
            setLoading(false);
        });
    }

    useEffect(() => {
        updateTradeRuleConditions();
    }, []);

    return (
        <div>
            <LoadingBar active={loading} />
            <div className='mt-3 mb-3 control'>
                <h4>Conditions</h4>
                <Button variant="contained" onClick={newTradeRuleCondition} disabled={loading}>+ Add new condition</Button>
            </div>
            {tradeRuleConditions.length > 0 && Object.keys(tradeRuleConditionAttributes).length > 0 &&
                <TradeRuleConditionTable
                    tradeRuleConditions={tradeRuleConditions}
                    tradeRuleConditionAttributes={tradeRuleConditionAttributes}
                    updateTradeRuleConditions={updateTradeRuleConditions}
                    runningTest={runningTest}
                />
            }
        </div>
    )
};

export default TradeRuleConditions;
