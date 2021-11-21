import React, { useState } from 'react';
import TextField from '@mui/material/TextField';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';
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
            TradeRuleConditionService.updateTradeRuleCondition(tradeRuleCondition).then((result) => {
                setLoading(false);
            });
        }
    }

    if (tradeRuleCondition !== undefined) {
        return (
            <div>
                <div className='table-form'>
                    <div className='trc-form'>
                        <div className='trc-section'>
                            <TextField sx={{ width: '52%' }} id="trc-description" label="Description" variant="outlined" value={tradeRuleCondition.description}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, description: e.target.value })}
                            />
                            <FormControl sx={{ width: '16%' }}>
                                <InputLabel id="tr-candleStickValueTypeId-label">Value type</InputLabel>
                                <Select labelId="tr-candleStickValueTypeId" id="tr-candleStickValueTypeId-select" value={tradeRuleCondition.candleStickValueTypeId} label="Value type"
                                    onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, candleStickValueTypeId: e.target.value })} >
                                    {tradeRuleConditionAttributes.CandleStickValueType.map((candleStickValueType) => (
                                        <MenuItem key={candleStickValueType.id} value={candleStickValueType.id}> {candleStickValueType.name} </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                            <FormControl sx={{ width: '16%' }}>
                                <InputLabel id="tr-tradeRuleConditionComparatorId-label">Con.comp</InputLabel>
                                <Select labelId="tr-tradeRuleConditionComparatorId" id="tr-tradeRuleConditionComparatorId-select" value={tradeRuleCondition.tradeRuleConditionComparatorId} label="Con.comp"
                                    onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, tradeRuleConditionComparatorId: e.target.value })} >
                                    {tradeRuleConditionAttributes.TradeRuleConditionComparator.map((tradeRuleConditionComparator) => (
                                        <MenuItem key={tradeRuleConditionComparator.id} value={tradeRuleConditionComparator.id}> {tradeRuleConditionComparator.name} </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                            <FormControl sx={{ width: '16%' }}>
                                <InputLabel id="tr-tradeRuleConditionSampleDirectionId-label">Con.sdir</InputLabel>
                                <Select labelId="tr-tradeRuleConditionSampleDirectionId" id="tr-tradeRuleConditionSampleDirectionId-select" value={tradeRuleCondition.tradeRuleConditionSampleDirectionId} label="Con.sdir"
                                    onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, tradeRuleConditionSampleDirectionId: e.target.value })} >
                                    {tradeRuleConditionAttributes.TradeRuleConditionSampleDirection.map((tradeRuleConditionSampleDirection) => (
                                        <MenuItem key={tradeRuleConditionSampleDirection.id} value={tradeRuleConditionSampleDirection.id}> {tradeRuleConditionSampleDirection.name} </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </div>
                        <div className='trc-section'>
                            <TextField sx={{ width: '20%' }} id="tr-fromMinutesOffset" label="From" variant="outlined" type="number" value={tradeRuleCondition.fromMinutesOffset}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, fromMinutesOffset: e.target.value })} />
                            <TextField sx={{ width: '20%' }} id="tr-toMinutesOffset" label="To" variant="outlined" type="number" value={tradeRuleCondition.toMinutesOffset}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, toMinutesOffset: e.target.value })} />
                            <TextField sx={{ width: '20%' }} id="tr-fromMinutesOffset" label="From sample" variant="outlined" type="number" value={tradeRuleCondition.fromMinutesSample}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, fromMinutesSample: e.target.value })} />
                            <TextField sx={{ width: '20%' }} id="tr-toMinutesOffset" label="From sample" variant="outlined" type="number" value={tradeRuleCondition.toMinutesSample}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, toMinutesSample: e.target.value })} />
                            <TextField sx={{ width: '20%' }} id="tr-deltaPercent" label="% change" variant="outlined" type="number" value={tradeRuleCondition.deltaPercent}
                                onChange={e => setTradeRuleCondition({ ...tradeRuleCondition, deltaPercent: e.target.value })} />
                        </div>
                    </div>
                    <div className="actions">
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
