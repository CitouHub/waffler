import React, { useState, useEffect } from 'react';
import TextField from '@mui/material/TextField';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faTrashAlt, faPlayCircle, faStopCircle, faSave } from "@fortawesome/free-solid-svg-icons";
import ProgressBar from '../../../components/utils/progressbar';

import TradeRuleService from '../../../services/traderule.service';
import TradeRuleTestDialog from '../form/traderuletest.dialog';

import './form.css';

const TradeRuleForm = ({ data, tradeRuleAttributes, updateTradeRules }) => {
    const [loading, setLoading] = useState(false);
    const [tradeRule, setTradeRule] = useState(data);
    const [traderRuleTestStatus, setTraderRuleTestStatus] = useState({});
    const [dialogOpen, setDialogOpen] = useState(false);
    const [runningTest, setRunningTest] = useState(data.testTradeInProgress);

    useEffect(() => {
        getTradeRuleTestStatus();
    }, [runningTest]);

    const getTradeRuleTestStatus = () => {
        if (runningTest === true) {
            TradeRuleService.getTradeRuleTestStatus(tradeRule.id).then((result) => {
                if (result !== undefined) {
                    setTraderRuleTestStatus(result);

                    if (result.aborted === true) {
                        setRunningTest(false);
                    }

                    if (result.progress < 100 && result.aborted === false) {
                        setTimeout(() => getTradeRuleTestStatus(), 800);
                    } else {
                        setTraderRuleTestStatus({});
                    }
                }
            });
        }
    }

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
            TradeRuleService.updateTradeRule(tradeRule).then((result) => {
                setLoading(false);
            });
        }
    }

    const startTradeRuleTest = (tradeRuleTest) => {
        TradeRuleService.startTradeRuleTest({ ...tradeRuleTest, tradeRuleId: tradeRule.id }).then((result) => {
            setDialogOpen(false);
            setRunningTest(true);
        });
    }

    const stopTradeRuleTest = () => {
        TradeRuleService.abortTradeRuleTest(tradeRule.id).then(() => {
            setRunningTest(false);
        });
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
                    <FormControl sx={{ width: '10%' }}>
                        <InputLabel id="tr-type">Type</InputLabel>
                        <Select labelId="tr-type" id="tr-type-select" value={tradeRule.tradeTypeId} label="Type"
                            onChange={e => setTradeRule({ ...tradeRule, tradeTypeId: e.target.value })} >
                            {tradeRuleAttributes.TradeType.map((tradeType) => (
                                <MenuItem key={tradeType.id} value={tradeType.id}> {tradeType.name} </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <FormControl sx={{ width: '10%' }}>
                        <InputLabel id="tr-type">Co.opr</InputLabel>
                        <Select labelId="tr-type" id="tr-type-select" value={tradeRule.tradeConditionOperatorId} label="Co.opr"
                            onChange={e => setTradeRule({ ...tradeRule, tradeConditionOperatorId: e.target.value })} >
                            {tradeRuleAttributes.TradeConditionOperator.map((tradeConditionOperator) => (
                                <MenuItem key={tradeConditionOperator.id} value={tradeConditionOperator.id}> {tradeConditionOperator.name} </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <TextField sx={{ width: '10%' }} id="outlined-basic" label="Amount" variant="outlined" type="number" value={tradeRule.amount}
                        onChange={e => setTradeRule({ ...tradeRule, amount: e.target.value })} />
                    <TextField sx={{ width: '10%' }} id="outlined-basic" label="Min tr.int" variant="outlined" type="number" value={tradeRule.tradeMinIntervalMinutes}
                        onChange={e => setTradeRule({ ...tradeRule, tradeMinIntervalMinutes: e.target.value })} />
                    <FormControl sx={{ width: '10%' }}>
                        <InputLabel id="tr-type">Status</InputLabel>
                        <Select labelId="tr-type" id="tr-type-select" value={tradeRule.tradeRuleStatusId} label="Status"
                            onChange={e => setTradeRule({ ...tradeRule, tradeRuleStatusId: e.target.value })} >
                            {tradeRuleAttributes.TradeRuleStatus.map((tradeRuleStatus) => (
                                <MenuItem key={tradeRuleStatus.id} value={tradeRuleStatus.id}> {tradeRuleStatus.name} </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                    <div className="actions">
                        {!runningTest && <span className='fa-icon' onClick={saveTradeRule}><FontAwesomeIcon icon={faSave} className="mr-2" /></span>}
                        {!runningTest && <span className='fa-icon' onClick={deleteTradeRule}><FontAwesomeIcon icon={faTrashAlt} className="mr-2" /></span>}
                        {!runningTest && <span className='fa-icon' onClick={() => setDialogOpen(true)}><FontAwesomeIcon icon={faPlayCircle} className="mr-2" /></span>}
                        {runningTest && <span className='fa-icon' onClick={() => stopTradeRuleTest()}><FontAwesomeIcon icon={faStopCircle} className="mr-2" /></span>}
                    </div>
                </div>
                <ProgressBar progress={traderRuleTestStatus?.progress ?? 0} />
                <TradeRuleTestDialog dialogOpen={dialogOpen} setDialogOpen={setDialogOpen} startTradeRuleTest={startTradeRuleTest}/>
            </div>
        )
    } else {
        return null;
    }
};

export default TradeRuleForm;
