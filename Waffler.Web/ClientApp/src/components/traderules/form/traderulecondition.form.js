import React, { useState } from 'react';
import TextField from '@mui/material/TextField';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';
import InputAdornment from '@mui/material/InputAdornment';
import Switch from '@mui/material/Switch';
import FormControlLabel from '@mui/material/FormControlLabel';

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faTrashAlt, faSave } from "@fortawesome/free-solid-svg-icons";

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
                fromMinutesOffset: -1 * getMinutes(tradeRuleCondition.spanTimeUnit, tradeRuleCondition.fromTime),
                toMinutesOffset: -1 * getMinutes(tradeRuleCondition.spanTimeUnit, tradeRuleCondition.toTime),
                fromMinutesSample: getMinutes(tradeRuleCondition.sampleTimeUnit, tradeRuleCondition.fromSample),
                toMinutesSample: getMinutes(tradeRuleCondition.sampleTimeUnit, tradeRuleCondition.toSample),
            };

            TradeRuleConditionService.updateTradeRuleCondition(tradeRuleConditionUpdate).then((result) => {
                setLoading(false);
            });
        }
    }

    const getMinutes = (timeUnitId, value) => {
        switch (timeUnitId) {
            case 1: return value;
            case 2: return value*60;
            case 3: return value*60*24;
        }
    }

    const getTimeUnit = (timeUnitId) => {
        switch (timeUnitId) {
            case 1: return 'Minutes';
            case 2: return 'Hours';
            case 3: return 'Days';
        }
    }

    if (tradeRuleCondition !== undefined) {
        return (
            <div>
                <div className='table-form'>
                    <div className='trc-form'>
                        <div className='trc-section'>
                            <TextField sx={{ width: '80%' }} id="trc-description" label="Description" variant="outlined" value={tradeRuleCondition.description}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, description: e.target.value })}
                            />
                            <FormControl sx={{ width: '20%' }}>
                                <InputLabel id="tr-candleStickValueTypeId-label">Value type</InputLabel>
                                <Select labelId="tr-candleStickValueTypeId" id="tr-candleStickValueTypeId-select" value={tradeRuleCondition.candleStickValueTypeId} label="Value type"
                                    onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, candleStickValueTypeId: e.target.value })} >
                                    {tradeRuleConditionAttributes.CandleStickValueType.map((candleStickValueType) => (
                                        <MenuItem key={candleStickValueType.id} value={candleStickValueType.id}> {candleStickValueType.name} </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </div>
                        <div className='trc-section'>
                            <h5>Time span</h5>
                            <FormControl sx={{ width: '20%' }}>
                                <InputLabel id="tr-spanTimeUnit-label">Time unit</InputLabel>
                                <Select labelId="tr-spanTimeUnit" id="tr-spanTimeUnit-select" value={tradeRuleCondition.spanTimeUnit} label="Time unit"
                                    onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, spanTimeUnit: e.target.value })} >
                                    <MenuItem value={1}>Minute</MenuItem>
                                    <MenuItem value={2}>Hour</MenuItem>
                                    <MenuItem value={3}>Day</MenuItem>
                                </Select>
                            </FormControl>
                            <TextField sx={{ width: '20%' }} id="tr-fromMinutesOffset" label="From" variant="outlined" type="number" value={tradeRuleCondition.fromTime}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, fromTime: e.target.value })}
                                InputProps={{
                                    endAdornment: <InputAdornment position="end">{getTimeUnit(tradeRuleCondition.spanTimeUnit)} ago</InputAdornment>,
                                }} />
                            <TextField sx={{ width: '20%' }} id="tr-toMinutesOffset" label="To" variant="outlined" type="number" value={tradeRuleCondition.toTime}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, toTime: e.target.value })}
                                InputProps={{
                                    endAdornment: <InputAdornment position="end">{getTimeUnit(tradeRuleCondition.spanTimeUnit)} ago</InputAdornment>,
                                }} />
                            <FormControl sx={{ width: '20%' }}>
                                <InputLabel id="tr-tradeRuleConditionComparatorId-label">Condition comparator</InputLabel>
                                <Select labelId="tr-tradeRuleConditionComparatorId" id="tr-tradeRuleConditionComparatorId-select"
                                    value={tradeRuleCondition.tradeRuleConditionComparatorId} label="Condition comparator"
                                    onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, tradeRuleConditionComparatorId: e.target.value })} >
                                    {tradeRuleConditionAttributes.TradeRuleConditionComparator.map((tradeRuleConditionComparator) => (
                                        <MenuItem key={tradeRuleConditionComparator.id} value={tradeRuleConditionComparator.id}> {tradeRuleConditionComparator.name} </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                            <TextField sx={{ width: '20%' }} id="tr-deltaPercent" label="Change" variant="outlined" type="number" value={tradeRuleCondition.deltaPercent}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, deltaPercent: e.target.value })}
                                InputProps={{
                                    endAdornment: <InputAdornment position="end">%</InputAdornment>,
                                }} />
                        </div>
                        <div className='trc-section'>
                            <h5>Sampling</h5>
                            <FormControl sx={{ width: '20%' }}>
                                <InputLabel id="tr-sampleTimeUnit-label">Time unit</InputLabel>
                                <Select labelId="tr-sampleTimeUnit" id="tr-sampleTimeUnit-select" value={tradeRuleCondition.sampleTimeUnit} label="Time unit"
                                    onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, sampleTimeUnit: e.target.value })} >
                                    <MenuItem value={1}>Minute</MenuItem>
                                    <MenuItem value={2}>Hour</MenuItem>
                                    <MenuItem value={3}>Day</MenuItem>
                                </Select>
                            </FormControl>
                            <TextField sx={{ width: '20%' }} id="tr-fromMinutesSample" label="From" variant="outlined" type="number" value={tradeRuleCondition.fromSample}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, fromSample: e.target.value })}
                                InputProps={{
                                    endAdornment: <InputAdornment position="end">{getTimeUnit(tradeRuleCondition.sampleTimeUnit)}</InputAdornment>,
                                }} />
                            <TextField sx={{ width: '20%' }} id="tr-toMinutesSample" label="To" variant="outlined" type="number" value={tradeRuleCondition.toSample}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, toSample: e.target.value })}
                                InputProps={{
                                    endAdornment: <InputAdornment position="end">{getTimeUnit(tradeRuleCondition.sampleTimeUnit)}</InputAdornment>,
                                }} />
                            <FormControl sx={{ width: '20%' }}>
                                <InputLabel id="tr-tradeRuleConditionSampleDirectionId-label">Sample direction</InputLabel>
                                <Select labelId="tr-tradeRuleConditionSampleDirectionId" id="tr-tradeRuleConditionSampleDirectionId-select"
                                    value={tradeRuleCondition.tradeRuleConditionSampleDirectionId} label="Sample direction"
                                    onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, tradeRuleConditionSampleDirectionId: e.target.value })} >
                                    {tradeRuleConditionAttributes.TradeRuleConditionSampleDirection.map((tradeRuleConditionSampleDirection) => (
                                        <MenuItem key={tradeRuleConditionSampleDirection.id} value={tradeRuleConditionSampleDirection.id}> {tradeRuleConditionSampleDirection.name} </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
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
                    </div>
                    <div className="actions-bottom">
                        <span className='fa-icon' onClick={saveTradeRuleCondition}><FontAwesomeIcon icon={faSave} className="mr-2" /></span>
                        <span className='fa-icon' onClick={deleteTradeRuleCondition}><FontAwesomeIcon icon={faTrashAlt} className="mr-2" /></span>
                    </div>
                </div>
            </div>
        )
    } else {
        return null;
    }
};

export default TradeRuleConditionForm;
