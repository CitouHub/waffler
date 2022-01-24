import React, { useEffect, useState } from 'react';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';
import DateSpanFilter from './datespan.filter'
import MultiSelect from './input/multiselect'

import FilterCache from '../../util/filter.cache'

const TradeFilter = ({ updateFilter, filter, tradeRules, tradeOrderStatuses, simplified }) => {
    const [tradeOrderStatusMode, setTradeOrderStatusMode] = useState(FilterCache.getTradeOrderStatusMode());

    useEffect(() => {
        if (tradeRules && tradeRules.length > 0) {
            var cache = FilterCache.getSelectedTradeRules();
            updateFilter({ ...filter, selectedTradeRules: getSelection(tradeRules, cache) });
        }
    }, [tradeRules]);

    useEffect(() => {
        if (tradeOrderStatuses && tradeOrderStatuses.length > 0) {
            var cache = FilterCache.getSelectedTradeOrderStatuses();
            updateFilter({ ...filter, selectedTradeOrderStatuses: getSelection(tradeOrderStatuses, cache) });
        }
    }, [tradeOrderStatuses]);

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

    const updateTradeOrderStatusMode = (tradeOrderStatusMode) => {
        var simplifiedSelectedTradeOrderStatuses = [];
        if (tradeOrderStatusMode === 1 || tradeOrderStatusMode === 3) {
            tradeOrderStatuses.filter(_ => _.id <= 6).map(_ => simplifiedSelectedTradeOrderStatuses.push(_));
        }

        if (tradeOrderStatusMode === 2 || tradeOrderStatusMode === 3) {
            tradeOrderStatuses.filter(_ => _.id === 10).map(_ => simplifiedSelectedTradeOrderStatuses.push(_));
        }

        updateFilter({ ...filter, selectedTradeOrderStatuses: simplifiedSelectedTradeOrderStatuses });
        FilterCache.setTradeOrderStatusMode(tradeOrderStatusMode);
        setTradeOrderStatusMode(tradeOrderStatusMode);
    }

    return (
        <div className="d-flex">
            <DateSpanFilter filter={filter} updateFilter={updateFilter} />
            <MultiSelect
                label="Trade rules"
                items={tradeRules}
                selectedItems={filter.selectedTradeRules}
                updateSelectedItems={updateSelectedTradeRules} />
            {!simplified && <MultiSelect label="Trade order statuses"
                items={tradeOrderStatuses}
                selectedItems={filter.selectedTradeOrderStatuses}
                updateSelectedItems={updateSelectedTradeStatuses} />}
            {simplified && <div className="ml-2 mt-2 mb-2">
                <FormControl>
                    <InputLabel id="filter-trade-order-simplified-label">Include</InputLabel>
                    <Select sx={{ width: 300 }} labelId="filter-trade-order-simplified-label" id="filter-trade-order-simplified-select" value={tradeOrderStatusMode} label="Include"
                        onChange={e => { updateTradeOrderStatusMode(e.target.value) }} >
                        <MenuItem value={1}>Live orders</MenuItem>
                        <MenuItem value={2}>Test orders</MenuItem>
                        <MenuItem value={3}>Both</MenuItem>
                    </Select>
                </FormControl>
            </div>}
        </div>
    )
};

export default TradeFilter;