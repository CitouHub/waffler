﻿import React from 'react';
import TextField from '@mui/material/TextField';
import TradeRuleMultiSelect from './traderule.multiselect';
import TradeOrderStatusMultiSelect from './tradeorderstatus.multiselect';

const ChartFilter = ({
    updateFilter,
    filter,
    tradeRules, selectedTradeRules, updateSelectedTradeRules,
    tradeOrderStatuses, selectedTradeOrderStatuses, updateSelectedTradeStatuses
}) => {
    return (
        <div className="d-flex">
            <div className="m-2">
                <TextField
                    id="filter-fromDate"
                    label="From"
                    type="date"
                    defaultValue={filter?.fromDate?.toJSON()?.slice(0, 10)}
                    sx={{ width: 220 }}
                    InputLabelProps={{
                        shrink: true,
                    }}
                    onChange={e => updateFilter({ ...filter, fromDate: new Date(e.target.value) })}
                />
            </div>
            <div className="m-2">
                <TextField
                    id="filter-toDate"
                    label="To"
                    type="date"
                    defaultValue={filter?.toDate?.toJSON()?.slice(0, 10)}
                    sx={{ width: 220 }}
                    InputLabelProps={{
                        shrink: true,
                    }}
                    onChange={e => updateFilter({ ...filter, toDate: new Date(e.target.value) })}
                />
            </div>
            <div className="m-2">
                <TradeRuleMultiSelect tradeRules={tradeRules} selectedTradeRules={selectedTradeRules} updateSelectedTradeRules={updateSelectedTradeRules}/>
            </div>
            <div className="m-2">
                <TradeOrderStatusMultiSelect tradeOrderStatuses={tradeOrderStatuses} selectedTradeOrderStatuses={selectedTradeOrderStatuses} updateSelectedTradeStatuses={updateSelectedTradeStatuses} />
            </div>
        </div>
    )
};

export default ChartFilter;
