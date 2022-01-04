import React from 'react';
import DateSpanFilter from './datespan.filter'
import MultiSelect from './input/multiselect'

const TradeFilter = ({
    updateFilter,
    filter,
    tradeRules, selectedTradeRules, updateSelectedTradeRules,
    tradeOrderStatuses, selectedTradeOrderStatuses, updateSelectedTradeStatuses
}) => {
    return (
        <div className="d-flex">
            <DateSpanFilter filter={filter} updateFilter={updateFilter} />
            <MultiSelect
                label="Trade rules"
                items={tradeRules}
                selectedItems={selectedTradeRules}
                updateSelectedItems={updateSelectedTradeRules} />
            <MultiSelect label="Trade order statuses"
                items={tradeOrderStatuses}
                selectedItems={selectedTradeOrderStatuses}
                updateSelectedItems={updateSelectedTradeStatuses} />
        </div>
    )
};

export default TradeFilter;