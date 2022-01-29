import React, { useEffect, useState } from 'react';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';
import DateSpanFilter from './datespan.filter';
import MultiSelect from './input/multiselect';

import TradeRuleService from '../../services/traderule.service';
import TradeOrderService from '../../services/tradeorder.service';
import FilterCache from '../../util/filter.cache';

const TradeFilter = ({ simplified, datesChanged, selectionsChanged }) => {
    const [loading, setLoading] = useState(true);
    const [tradeRules, setTradeRules] = useState([]);
    const [tradeOrderStatuses, setTradeOrderStatuses] = useState([]);
    const [tradeOrderStatusMode, setTradeOrderStatusMode] = useState(FilterCache.getTradeOrderStatusMode());
    const [filter, setFilter] = useState({
        fromDate: null,
        toDate: new Date(),
        selectedTradeRules: [],
        selectedTradeOrderStatuses: [],
    });

    useEffect(() => {
        setLoading(true);
        var getTradeRules = TradeRuleService.getTradeRules();
        var getTradeOrderStatuses = TradeOrderService.getTradeOrderStatuses();

        Promise.all([getTradeRules, getTradeOrderStatuses]).then((result) => {

            result[0].push({
                id: 0,
                name: 'Manual'
            });
            setTradeRules(result[0]);
            setTradeOrderStatuses(result[1]);

            var cachedFromDate = FilterCache.getFromDate();
            if (!cachedFromDate) {
                let cachedFromDate = new Date();
                cachedFromDate.setDate(cachedFromDate.getDate() - 90);
            }
            var cacheSelectedTradeRules = FilterCache.getSelectedTradeRules();
            var cacheSelectedTradeOrderStatuses = FilterCache.getSelectedTradeOrderStatuses();

            setFilter({
                fromDate: cachedFromDate,
                toDate: new Date(),
                selectedTradeRules: getSelection(tradeRules, cacheSelectedTradeRules),
                selectedTradeOrderStatuses: getSelection(tradeOrderStatuses, cacheSelectedTradeOrderStatuses),
            });

            setLoading(false);
            datesChanged(filter);
            selectionsChanged(filter);
        });
    }, []);

    useEffect(() => {
        if (loading === false) {
            FilterCache.setFromDate(filter.fromDate);
            datesChanged(filter);
        }
    }, [filter.fromDate, filter.fromDate]);

    useEffect(() => {
        if (loading === false) {
            FilterCache.setSelectedTradeRules(filter.selectedTradeRules);
            FilterCache.setSelectedTradeOrderStatuses(filter.selectedTradeOrderStatuses);
            selectionsChanged(filter);
        }
    }, [filter.selectedTradeRules, filter.selectedTradeOrderStatuses]);

    const getSelection = (items, cache) => {
        return items.filter((item) => {
            if (cache.filter((cached) => cached.id === item.id).length > 0) {
                return item;
            }
        });
    }

    const updateTradeOrderStatusMode = (tradeOrderStatusMode) => {
        var simplifiedSelectedTradeOrderStatuses = [];
        if (tradeOrderStatusMode === 1 || tradeOrderStatusMode === 3) {
            tradeOrderStatuses.filter(_ => _.id <= 6).map(_ => simplifiedSelectedTradeOrderStatuses.push(_));
        }

        if (tradeOrderStatusMode === 2 || tradeOrderStatusMode === 3) {
            tradeOrderStatuses.filter(_ => _.id === 10).map(_ => simplifiedSelectedTradeOrderStatuses.push(_));
        }

        setFilter({ ...filter, selectedTradeOrderStatuses: simplifiedSelectedTradeOrderStatuses });
        FilterCache.setTradeOrderStatusMode(tradeOrderStatusMode);
        setTradeOrderStatusMode(tradeOrderStatusMode);
    }

    return (
        <div className="d-flex">
            {!loading && <React.Fragment>
                <DateSpanFilter
                    fromDate={filter.fromDate}
                    toDate={filter.toDate}
                    updateFromDate={newDate => { setFilter({ ...filter, fromDate: newDate }) }}
                    updateToDate={newDate => { setFilter({ ...filter, toDate: newDate }) }} />
                <MultiSelect
                    label="Trade rules"
                    items={tradeRules}
                    selectedItems={filter.selectedTradeRules}
                    updateSelectedItems={newSelection => { setFilter({ ...filter, selectedTradeRules: newSelection }) }} />
                {!simplified && <MultiSelect label="Trade order statuses"
                    items={tradeOrderStatuses}
                    selectedItems={filter.selectedTradeOrderStatuses}
                    updateSelectedItems={newSelection => { setFilter({ ...filter, selectedTradeOrderStatuses: newSelection }) }} />}
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
            </React.Fragment>}
        </div>
    )
};

export default TradeFilter;