import React, { useState, useEffect } from 'react';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';

import TradeRuleService from '../../../services/traderule.service';

import './dialog.css';

const TradeRuleDialog = ({ tradeOrder, closeDialog, setTradeRule }) => {
    const [selectedTradeRule, setSelectedTradeRule] = useState({id: 0, name: 'Manual'});
    const [tradeRules, setTradeRules] = useState([]);

    useEffect(() => {
        TradeRuleService.getTradeRules().then((result) => {
            result.push({
                id: 0,
                name: 'Manual'
            });
            setTradeRules(result);
        });
    }, []);

    const updateSelectedTradeRule = (tradeRuleId) => {
        const tradeRule = tradeRules.find(_ => _.id === tradeRuleId);
        setSelectedTradeRule(
            {
                id: tradeRule.id,
                name: tradeRule.name
            });
    }
   
    return (
        <div>
            <Dialog open={tradeOrder.id !== undefined}>
                <DialogTitle>Set trade rule</DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        Do you want to link this manual order to a trade rule.
                    </DialogContentText>
                    <div className='mt-4'>
                        {tradeRules && tradeRules.length > 0 && <FormControl sx={{ width: '300px' }}>
                            <InputLabel id="set-traderule-label">Trade rule</InputLabel>
                            <Select labelId="set-traderule-label" id="set-traderule-select" value={selectedTradeRule.id} label="Trade rule"
                                onChange={e => updateSelectedTradeRule(e.target.value)}>
                                {tradeRules.map((tradeRule) => (
                                    <MenuItem key={tradeRule.id} value={tradeRule.id} name={tradeRule.name}> {tradeRule.name} </MenuItem>
                                ))}
                            </Select>
                        </FormControl>}
                    </div>
                </DialogContent>
                <DialogActions>
                    <div className='dialog-control'>
                        <Button className='m-3' variant="outlined" onClick={closeDialog}>Cancel</Button>
                        <Button className='m-3' variant="contained" onClick={() => {
                            setTradeRule(tradeOrder.id, selectedTradeRule);
                            closeDialog();
                        }}>Set trade rule</Button>
                    </div>
                </DialogActions>
            </Dialog>
        </div>
    );
}

export default TradeRuleDialog;