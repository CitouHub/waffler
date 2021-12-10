﻿import React, { useState } from 'react';
import TextField from '@mui/material/TextField';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';
import InputAdornment from '@mui/material/InputAdornment';
import TimeUnit from './timeunit';
import TradeRuleActionMenu from './traderule.action.menu';

import TradeRuleService from '../../../services/traderule.service';

import './form.css';

const TradeRuleForm = ({ data, tradeRuleAttributes, updateTradeRules, openStartTestDialog, stopTradeRuleTest, runningTest }) => {
    const [loading, setLoading] = useState(false);
    const [tradeRule, setTradeRule] = useState(data);

    const deleteTradeRule = () => {
        if (loading === false && runningTest === false) {
            setLoading(true);
            TradeRuleService.deleteTradeRule(tradeRule.id).then((result) => {
                if (result === true) {
                    updateTradeRules();
                }
                setLoading(false);
            });
        }
    }

    const saveTradeRule = () => {
        if (loading === false && runningTest === false) {
            setLoading(true);

            const tradeRuleUpdate = {
                ...tradeRule,
                tradeMinIntervalMinutes: TimeUnit.getMinutes(tradeRule.intervalTimeUnit, tradeRule.tradeMinInterval),
                tradeOrderExpirationMinutes: TimeUnit.getMinutes(tradeRule.orderExpirationTimeUnit, tradeRule.orderExpiration)
            };

            TradeRuleService.updateTradeRule(tradeRuleUpdate).then((result) => {
                setLoading(false);
            });
        }
    }

    const copyTradeRule = () => {
        if (loading === false) {
            setLoading(true);

            TradeRuleService.copyTradeRule(tradeRule.id).then((result) => {
                if (result === true) {
                    updateTradeRules();
                }
                setLoading(false);
            });
        }
    }

    const exportTradeRule = () => {
        if (loading === false) {
            setLoading(true);

            TradeRuleService.getTradeRule(tradeRule.id).then((result) => {
                exportFile(result);
                setLoading(false);
            });
        }
    }

    const exportFile = async (exportTradeRule) => {
        const json = JSON.stringify(exportTradeRule, null, 2);
        const blob = new Blob([json], { type: 'application/json' });
        const href = await URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = href;
        link.download = `${tradeRule.name}.json`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }

    if (tradeRule !== undefined) {
        return (
            <div>
                <div className='d-flex'>
                    <div className='table-form'>
                        <div className='table-form-section'>
                            <TextField sx={{ width: '60%' }} id="tr-name" label="Name" variant="outlined" value={tradeRule.name}
                                onChange={e => setTradeRule({ ...tradeRule, name: e.target.value })}
                            />
                            <FormControl sx={{ width: '20%' }}>
                                <InputLabel id="tr-conditionOperator-label">Condition operator</InputLabel>
                                <Select labelId="tr-conditionOperator-label" id="tr-conditionOperator-select" value={tradeRule.tradeConditionOperatorId} label="Condition operator"
                                    onChange={e => setTradeRule({ ...tradeRule, tradeConditionOperatorId: e.target.value })} >
                                    {tradeRuleAttributes.TradeConditionOperator.map((tradeConditionOperator) => (
                                        <MenuItem key={tradeConditionOperator.id} value={tradeConditionOperator.id}> {tradeConditionOperator.name} </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                            <FormControl sx={{ width: '20%' }}>
                                <InputLabel id="tr-status-label">Status</InputLabel>
                                <Select labelId="tr-status-label" id="tr-status-select" value={tradeRule.tradeRuleStatusId} label="Status"
                                    onChange={e => setTradeRule({ ...tradeRule, tradeRuleStatusId: e.target.value })} >
                                    {tradeRuleAttributes.TradeRuleStatus.map((tradeRuleStatus) => (
                                        <MenuItem key={tradeRuleStatus.id} value={tradeRuleStatus.id}> {tradeRuleStatus.name} </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </div>
                        <div className='table-form-section'>
                            <h5>Trade settings</h5>
                            <FormControl sx={{ width: '20%' }}>
                                <InputLabel id="tr-action-label">Action</InputLabel>
                                <Select labelId="tr-action-label" id="tr-action-select" value={tradeRule.tradeActionId} label="Action"
                                    onChange={e => setTradeRule({ ...tradeRule, tradeActionId: e.target.value })} >
                                    {tradeRuleAttributes.TradeAction.map((tradeAction) => (
                                        <MenuItem key={tradeAction.id} value={tradeAction.id}> {tradeAction.name} </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                            <TextField sx={{ width: '20%' }} id="tr-amount" label="Amount" variant="outlined" type="number" value={tradeRule.amount}
                                onChange={e => setTradeRule({ ...tradeRule, amount: e.target.value })}
                                InputProps={{
                                    endAdornment: <InputAdornment position="end">€</InputAdornment>,
                                }} />
                            <FormControl sx={{ width: '20%' }}>
                                <InputLabel id="tr-action-label">Price reference</InputLabel>
                                <Select labelId="tr-action-label" id="tr-action-select" value={tradeRule.candleStickValueTypeId} label="Price reference"
                                    onChange={e => setTradeRule({ ...tradeRule, candleStickValueTypeId: e.target.value })} >
                                    {tradeRuleAttributes.CandleStickValueType.map((candleStickValueType) => (
                                        <MenuItem key={candleStickValueType.id} value={candleStickValueType.id}> {candleStickValueType.name} </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                            <TextField sx={{ width: '20%' }} id="tr-priceDeltaPercent" label="Price offset" variant="outlined" type="number" value={tradeRule.priceDeltaPercent}
                                onChange={e => setTradeRule({ ...tradeRule, priceDeltaPercent: e.target.value })}
                                InputProps={{
                                    endAdornment: <InputAdornment position="end">%</InputAdornment>,
                                }} />
                        </div>
                        <div className='table-form-section'>
                            <h5>Order timing</h5>
                            <FormControl sx={{ width: '20%' }}>
                                <InputLabel id="tr-intervalTimeUnit-label">Interval unit</InputLabel>
                                <Select labelId="tr-intervalTimeUnit-label" id="tr-intervalTimeUnit-select" value={tradeRule.intervalTimeUnit} label="Interval unit"
                                    onChange={e => setTradeRule({ ...tradeRule, intervalTimeUnit: e.target.value })} >
                                    <MenuItem value={1}>Minute</MenuItem>
                                    <MenuItem value={2}>Hour</MenuItem>
                                    <MenuItem value={3}>Day</MenuItem>
                                    <MenuItem value={4}>Week</MenuItem>
                                </Select>
                            </FormControl>
                            <TextField sx={{ width: '20%' }} id="tr-minimumInterval" label="Minimum interval" variant="outlined" type="number" value={tradeRule.tradeMinInterval}
                                onChange={e => setTradeRule({ ...tradeRule, tradeMinInterval: e.target.value })}
                                InputProps={{
                                    endAdornment: <InputAdornment position="end">{TimeUnit.getUnit(tradeRule.intervalTimeUnit)}</InputAdornment>,
                                }} />
                            <FormControl sx={{ width: '20%' }}>
                                <InputLabel id="tr-orderExpirationTimeUnit-label">Order expiration unit</InputLabel>
                                <Select labelId="tr-orderExpirationTimeUnit-label" id="tr-orderExpirationTimeUnit-select" value={tradeRule.orderExpirationTimeUnit} label="Order expiration unit"
                                    onChange={e => setTradeRule({ ...tradeRule, orderExpirationTimeUnit: e.target.value })} >
                                    <MenuItem value={1}>Minute</MenuItem>
                                    <MenuItem value={2}>Hour</MenuItem>
                                    <MenuItem value={3}>Day</MenuItem>
                                    <MenuItem value={4}>Week</MenuItem>
                                </Select>
                            </FormControl>
                            <TextField sx={{ width: '20%' }} id="tr-orderExpiration" label="Order expiration" variant="outlined" type="number" value={tradeRule.orderExpiration}
                                onChange={e => setTradeRule({ ...tradeRule, orderExpiration: e.target.value })}
                                InputProps={{
                                    endAdornment: <InputAdornment position="end">{TimeUnit.getUnit(tradeRule.orderExpirationTimeUnit)}</InputAdornment>,
                                }} />
                        </div>
                    </div>
                    <div className="actions-top">
                        <TradeRuleActionMenu
                            runningTest={runningTest}
                            saveTradeRule={saveTradeRule}
                            copyTradeRule={copyTradeRule}
                            exportTradeRule={exportTradeRule}
                            deleteTradeRule={deleteTradeRule}
                            startTradeRuleTest={() => openStartTestDialog()}
                            stopTradeRuleTest={() => stopTradeRuleTest()}
                        />
                    </div>
                </div>
            </div>
        )
    } else {
        return null;
    }
};

export default TradeRuleForm;
