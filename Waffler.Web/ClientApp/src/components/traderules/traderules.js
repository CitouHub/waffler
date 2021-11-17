import React, { useState, useEffect } from "react";

import LoadingBar from '../../components/utils/loadingbar';
import TradeRuleService from '../../services/traderule.service';
import TradeRuleTable from './table/traderule.table';
import TradeRuleForm from './form/traderule.form';

const TradeRules = (props) => {
    const [loading, setLoading] = useState(true);
    const [tradeRules, setTradeRules] = useState({});

    useEffect(() => {
        TradeRuleService.getTradeRules().then((result) => {
            setTradeRules(result);
            setLoading(false);
        });
    }, []);

    if (loading) {
        return (
            <div>
                <LoadingBar active={loading} />
            </div>
        )
    } else if (tradeRules.length > 0) {
        return (
            <div>
                <TradeRuleTable data={tradeRules}/>
            </div>
        )
    }
};

export default TradeRules;
