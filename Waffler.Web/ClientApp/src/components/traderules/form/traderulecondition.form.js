import React, { useState } from 'react';
import TextField from '@mui/material/TextField';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';
import InputAdornment from '@mui/material/InputAdornment';
import Switch from '@mui/material/Switch';
import FormControlLabel from '@mui/material/FormControlLabel';
import TimeUnit from './timeunit';
import TradeRuleConditionActionMenu from './traderulecondition.action.menu';
import TimeUnitSelect from './timeunit.select';

import TradeRuleConditionService from '../../../services/traderulecondition.service';

import './form.css';

const TradeRuleConditionForm = ({ data, tradeRuleConditionAttributes, updateTradeRuleConditions }) => {
    const [loading, setLoading] = useState(false);
    const [tradeRuleCondition, setTradeRuleCondition] = useState(data);

    const deleteTradeRuleCondition = () => {
        if (loading === false) {
            setLoading(true);
            TradeRuleConditionService.deleteTradeRuleCondition(tradeRuleCondition.id).then((result) => {
                if (result === true) {
                    updateTradeRuleConditions();
                }
                setLoading(false);
            });
        }
    }

    const saveTradeRuleCondition = () => {
        if (loading === false) {
            setLoading(true);

            const tradeRuleConditionUpdate = {
                ...tradeRuleCondition,
                fromMinutes: -1 * TimeUnit.getMinutes(tradeRuleCondition.fromTimeUnit, tradeRuleCondition.fromTime),
                fromPeriodMinutes: TimeUnit.getMinutes(tradeRuleCondition.fromPeriodTimeUnit, tradeRuleCondition.fromPeriod),
                toMinutes: -1 * TimeUnit.getMinutes(tradeRuleCondition.toTimeUnit, tradeRuleCondition.toTime),
                toPeriodMinutes: TimeUnit.getMinutes(tradeRuleCondition.toPeriodTimeUnit, tradeRuleCondition.toPeriod),
            };

            TradeRuleConditionService.updateTradeRuleCondition(tradeRuleConditionUpdate).then((result) => {
                setLoading(false);
            });
        }
    }

    if (tradeRuleCondition !== undefined) {
        return (
            <div>
                <div className='d-flex'>
                    <div className='table-form'>
                        <div className='table-form-section'>
                            <TextField sx={{ width: '48%' }} id="trc-description" label="Description" variant="outlined" value={tradeRuleCondition.description}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, description: e.target.value })}
                            />
                            <FormControl sx={{ width: '16%' }}>
                                <InputLabel id="trc-tradeRuleConditionComparator-label">Condition comparator</InputLabel>
                                <Select labelId="trc-tradeRuleConditionComparator-label" id="trc-tradeRuleConditionComparator-select"
                                    value={tradeRuleCondition.tradeRuleConditionComparatorId} label="Condition comparator"
                                    onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, tradeRuleConditionComparatorId: e.target.value })} >
                                    {tradeRuleConditionAttributes.TradeRuleConditionComparator.map((tradeRuleConditionComparator) => (
                                        <MenuItem key={tradeRuleConditionComparator.id} value={tradeRuleConditionComparator.id}> {tradeRuleConditionComparator.name} </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                            <TextField sx={{ width: '16%' }} id="trc-deltaPercent" label="Change" variant="outlined" type="number" value={tradeRuleCondition.deltaPercent}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, deltaPercent: e.target.value })}
                                InputProps={{
                                    endAdornment: <InputAdornment position="end">%</InputAdornment>,
                                }} />
                            <FormControl sx={{ width: '20%', paddingTop: '9px', paddingLeft: '1rem' }}>
                                <div>
                                    <FormControlLabel
                                        control={
                                            <Switch checked={tradeRuleCondition.isOn}
                                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, isOn: e.target.checked })}
                                                name="trc-active" />
                                        }
                                        label={tradeRuleCondition.isOn ? 'Condition active' : 'Condition inactive'}
                                    />
                                </div>
                            </FormControl>
                        </div>

                        <div className='table-form-section'>
                            <h5>From</h5>
                            <FormControl sx={{ width: '16%' }}>
                                <InputLabel id="trc-fromCandleStickValueType-label">Price type</InputLabel>
                                <Select labelId="trc-fromCandleStickValueType-label" id="trc-fromCandleStickValueType-select" value={tradeRuleCondition.fromCandleStickValueTypeId} label="Price type"
                                    onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, fromCandleStickValueTypeId: e.target.value })} >
                                    {tradeRuleConditionAttributes.CandleStickValueType.map((candleStickValueType) => (
                                        <MenuItem key={candleStickValueType.id} value={candleStickValueType.id}> {candleStickValueType.name} </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                            <TimeUnitSelect label='From unit' width='16%' timeUnit={tradeRuleCondition.fromTimeUnit} onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, fromTimeUnit: e.target.value })} />
                            <TextField sx={{ width: '16%' }} id="trc-fromTime" label="From" variant="outlined" type="number" value={tradeRuleCondition.fromTime}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, fromTime: e.target.value })}
                                InputProps={{
                                    endAdornment: <InputAdornment position="end">{TimeUnit.getUnit(tradeRuleCondition.fromTimeUnit)} ago</InputAdornment>,
                                }} />
                            <FormControl sx={{ width: '16%' }}>
                                <InputLabel id="trc-fromTradeRuleConditionPeriodDirection-label">Period direction</InputLabel>
                                <Select labelId="trc-fromTradeRuleConditionPeriodDirection-label" id="trc-fromnTradeRuleConditionPeriodDirection-select"
                                    value={tradeRuleCondition.fromTradeRuleConditionPeriodDirectionId} label="Period direction"
                                    onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, fromTradeRuleConditionPeriodDirectionId: e.target.value })} >
                                    {tradeRuleConditionAttributes.TradeRuleConditionPeriodDirection.map((tradeRuleConditionPeriodDirection) => (
                                        <MenuItem key={tradeRuleConditionPeriodDirection.id} value={tradeRuleConditionPeriodDirection.id}> {tradeRuleConditionPeriodDirection.name} </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                            <TimeUnitSelect label='Period unit' width='16%' timeUnit={tradeRuleCondition.fromPeriodTimeUnit} onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, fromPeriodTimeUnit: e.target.value })} />
                            <TextField sx={{ width: '16%' }} id="trc-fromPeriod" label="Period" variant="outlined" type="number" value={tradeRuleCondition.fromPeriod}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, fromPeriod: e.target.value })}
                                InputProps={{
                                    endAdornment: <InputAdornment position="end">{TimeUnit.getUnit(tradeRuleCondition.fromPeriodTimeUnit)}</InputAdornment>,
                                }} />
                        </div>

                        <div className='table-form-section'>
                            <h5>To</h5>
                            <FormControl sx={{ width: '16%' }}>
                                <InputLabel id="trc-toCandleStickValueType-label">Price type</InputLabel>
                                <Select labelId="trc-toCandleStickValueType-label" id="trc-toCandleStickValueType-select" value={tradeRuleCondition.toCandleStickValueTypeId} label="Price type"
                                    onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, toCandleStickValueTypeId: e.target.value })} >
                                    {tradeRuleConditionAttributes.CandleStickValueType.map((candleStickValueType) => (
                                        <MenuItem key={candleStickValueType.id} value={candleStickValueType.id}> {candleStickValueType.name} </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                            <TimeUnitSelect label='To unit' width='16%' timeUnit={tradeRuleCondition.toTimeUnit} onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, toTimeUnit: e.target.value })} />
                            <TextField sx={{ width: '16%' }} id="trc-toTime" label="To" variant="outlined" type="number" value={tradeRuleCondition.toTime}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, toTime: e.target.value })}
                                InputProps={{
                                    endAdornment: <InputAdornment position="end">{TimeUnit.getUnit(tradeRuleCondition.toTimeUnit)} ago</InputAdornment>,
                                }} />
                            <FormControl sx={{ width: '16%' }}>
                                <InputLabel id="trc-toTradeRuleConditionPeriodDirection-label">Period direction</InputLabel>
                                <Select labelId="trc-toTradeRuleConditionPeriodDirection-label" id="trc-toTradeRuleConditionPeriodDirection-select"
                                    value={tradeRuleCondition.toTradeRuleConditionPeriodDirectionId} label="Period direction"
                                    onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, toTradeRuleConditionPeriodDirectionId: e.target.value })} >
                                    {tradeRuleConditionAttributes.TradeRuleConditionPeriodDirection.map((tradeRuleConditionPeriodDirection) => (
                                        <MenuItem key={tradeRuleConditionPeriodDirection.id} value={tradeRuleConditionPeriodDirection.id}> {tradeRuleConditionPeriodDirection.name} </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                            <TimeUnitSelect label='Period unit' width='16%' timeUnit={tradeRuleCondition.toPeriodTimeUnit} onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, toPeriodTimeUnit: e.target.value })} />
                            <TextField sx={{ width: '16%' }} id="trc-toPeriod" label="Period" variant="outlined" type="number" value={tradeRuleCondition.toPeriod}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, toPeriod: e.target.value })}
                                InputProps={{
                                    endAdornment: <InputAdornment position="end">{TimeUnit.getUnit(tradeRuleCondition.toPeriodTimeUnit)}</InputAdornment>,
                                }} />
                        </div>
                    </div>
                    <div className="actions-top">
                        <TradeRuleConditionActionMenu
                            saveTradeRuleCondition={saveTradeRuleCondition}
                            deleteTradeRuleCondition={deleteTradeRuleCondition}
                        />
                    </div>
                </div>
            </div>
        )
    } else {
        return null;
    }
};

export default TradeRuleConditionForm;
