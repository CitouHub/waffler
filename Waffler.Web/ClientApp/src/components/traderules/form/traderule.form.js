import React, { useState } from 'react';
import TextField from '@mui/material/TextField';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';
import InputAdornment from '@mui/material/InputAdornment';
import TimeUnit from './timeunit';

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faTrashAlt, faPlayCircle, faStopCircle, faSave } from "@fortawesome/free-solid-svg-icons";

import TradeRuleService from '../../../services/traderule.service';

import './form.css';

const TradeRuleForm = ({ data, tradeRuleAttributes, updateTradeRules, openStartTestDialog, stopTradeRuleTest, runningTest}) => {
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
                tradeMinIntervalMinutes: TimeUnit.getMinutes(tradeRule.intervalTimeUnit, tradeRule.tradeMinInterval)
            };

            TradeRuleService.updateTradeRule(tradeRuleUpdate).then((result) => {
                setLoading(false);
            });
        }
    }

    if (tradeRule !== undefined) {
        return (
            <div>
                <div className='table-form'>
                    <TextField sx={{ width: '32%' }} id="outlined-basic" label="Name" variant="outlined" value={tradeRule.name}
                        onChange={e => setTradeRule({ ...tradeRule, name: e.target.value })}
                    />
                    <FormControl sx={{ width: '10%' }}>
                        <InputLabel id="tr-action">Action</InputLabel>
                        <Select labelId="tr-action" id="tr-action-select" value={tradeRule.tradeActionId} label="Action"
                            onChange={e => setTradeRule({ ...tradeRule, tradeActionId: e.target.value })} >
                            {tradeRuleAttributes.TradeAction.map((tradeAction) => (
                                <MenuItem key={tradeAction.id} value={tradeAction.id}> {tradeAction.name} </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <TextField sx={{ width: '10%' }} id="outlined-basic" label="Amount" variant="outlined" type="number" value={tradeRule.amount}
                        onChange={e => setTradeRule({ ...tradeRule, amount: e.target.value })}
                        InputProps={{
                            endAdornment: <InputAdornment position="end">€</InputAdornment>,
                        }} />
                    <FormControl sx={{ width: '10%' }}>
                        <InputLabel id="tr-intervalTimeUnit-label">Interval unit</InputLabel>
                        <Select labelId="tr-intervalTimeUnit" id="tr-intervalTimeUnit-select" value={tradeRule.intervalTimeUnit} label="Interval unit"
                            onChange={e => setTradeRule({ ...tradeRule, intervalTimeUnit: e.target.value })} >
                            <MenuItem value={1}>Minute</MenuItem>
                            <MenuItem value={2}>Hour</MenuItem>
                            <MenuItem value={3}>Day</MenuItem>
                            <MenuItem value={4}>Week</MenuItem>
                        </Select>
                    </FormControl>
                    <TextField sx={{ width: '10%' }} id="outlined-basic" label="Minimum interval" variant="outlined" type="number" value={tradeRule.tradeMinInterval}
                        onChange={e => setTradeRule({ ...tradeRule, tradeMinInterval: e.target.value })}
                        InputProps={{
                            endAdornment: <InputAdornment position="end">{TimeUnit.getUnit(tradeRule.intervalTimeUnit)}</InputAdornment>,
                        }} />
                    <FormControl sx={{ width: '10%' }}>
                        <InputLabel id="tr-type">Condition operator</InputLabel>
                        <Select labelId="tr-type" id="tr-type-select" value={tradeRule.tradeConditionOperatorId} label="Condition operator"
                            onChange={e => setTradeRule({ ...tradeRule, tradeConditionOperatorId: e.target.value })} >
                            {tradeRuleAttributes.TradeConditionOperator.map((tradeConditionOperator) => (
                                <MenuItem key={tradeConditionOperator.id} value={tradeConditionOperator.id}> {tradeConditionOperator.name} </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <FormControl sx={{ width: '10%' }}>
                        <InputLabel id="tr-type">Status</InputLabel>
                        <Select labelId="tr-type" id="tr-type-select" value={tradeRule.tradeRuleStatusId} label="Status"
                            onChange={e => setTradeRule({ ...tradeRule, tradeRuleStatusId: e.target.value })} >
                            {tradeRuleAttributes.TradeRuleStatus.map((tradeRuleStatus) => (
                                <MenuItem key={tradeRuleStatus.id} value={tradeRuleStatus.id}> {tradeRuleStatus.name} </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <div className="actions-center">
                        {!runningTest && <span className='fa-icon' onClick={saveTradeRule}><FontAwesomeIcon icon={faSave} className="mr-2" /></span>}
                        {!runningTest && <span className='fa-icon' onClick={deleteTradeRule}><FontAwesomeIcon icon={faTrashAlt} className="mr-2" /></span>}
                        {!runningTest && <span className='fa-icon' onClick={() => openStartTestDialog()}><FontAwesomeIcon icon={faPlayCircle} className="mr-2" /></span>}
                        {runningTest && <span className='fa-icon' onClick={() => stopTradeRuleTest()}><FontAwesomeIcon icon={faStopCircle} className="mr-2" /></span>}
                    </div>
                </div>
            </div>
        )
    } else {
        return null;
    }
};

export default TradeRuleForm;
