import React, { useEffect } from 'react';
import DateSpanFilter from './datespan.filter'
import MultiSelect from './input/multiselect'

import FilterCache from '../../util/filter.cache'

const TradeFilter = ({ updateFilter, filter, tradeRules, tradeOrderStatuses }) => {

    useEffect(() => {
        if (filter.selectedTradeRules && filter.selectedTradeRules.length === 0 && tradeRules.length > 0) {
            var cache = FilterCache.getSelectedTradeRules();
            updateFilter({ ...filter, selectedTradeRules: getSelection(tradeRules, cache) });
        }
    }, [tradeRules, filter.selectedTradeRules]);

    useEffect(() => {
        if (filter.selectedTradeOrderStatuses && filter.selectedTradeOrderStatuses.length === 0 && tradeOrderStatuses.length > 0) {
            var cache = FilterCache.getSelectedTradeOrderStatuses();
            updateFilter({ ...filter, selectedTradeOrderStatuses: getSelection(tradeOrderStatuses, cache) });
        }
    }, [tradeOrderStatuses, filter.selectedTradeOrderStatuses]);

    useEffect(() => {
        if (!filter.fromDate) {
            var cachedFromDate = FilterCache.getFromDate();
            if (cachedFromDate) {
                updateFilter({ ...filter, fromDate: cachedFromDate });
            } else {
                let defaultFromDate = new Date();
                defaultFromDate.setDate(defaultFromDate.getDate() - 90);
                updateFilter({ ...filter, fromDate: defaultFromDate });
            }
        }
    }, []);

    useEffect(() => {
        FilterCache.setFromDate(filter.fromDate);
    }, [filter.fromDate]);

    const getSelection = (items, cache) => {
        return items.filter((item) => {
            if (cache.filter((cached) => cached.id === item.id).length > 0) {
                return item;
            }
        });
    }

    const updateSelectedTradeRules = (selectedTradeRules) => {
        FilterCache.setSelectedTradeRules(selectedTradeRules);
        updateFilter({ ...filter, selectedTradeRules: selectedTradeRules });
    }

    const updateSelectedTradeStatuses = (selectedTradeOrderStatuses) => {
        FilterCache.setSelectedTradeOrderStatuses(selectedTradeOrderStatuses);
        updateFilter({ ...filter, selectedTradeOrderStatuses: selectedTradeOrderStatuses });
    }

    return (
        <div className="d-flex">
            <DateSpanFilter filter={filter} updateFilter={updateFilter} />
            {tradeRules && tradeRules.length > 0 && <MultiSelect
                label="Trade rules"
                items={tradeRules}
                selectedItems={filter.selectedTradeRules}
                updateSelectedItems={updateSelectedTradeRules} />}
            {tradeOrderStatuses && tradeOrderStatuses.length > 0 && <MultiSelect label="Trade order statuses"
                items={tradeOrderStatuses}
                selectedItems={filter.selectedTradeOrderStatuses}
                updateSelectedItems={updateSelectedTradeStatuses} />}
        </div>
    )
};

export default TradeFilter;