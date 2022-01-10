import React, { useState } from 'react';
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

import './dialog.css';

const TradeRuleDialog = ({ tradeOrder, closeDialog, tradeRules, setTradeRule }) => {
    const [tradeRuleId, setTradeRuleId] = useState(0);
   
    return (
        <div>
            <Dialog open={tradeOrder.id !== undefined}>
                <DialogTitle>Set trade rule</DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        Do you want to link this manual order to a trade rule.
                    </DialogContentText>
                    <div className='mt-4'>
                        <FormControl sx={{ width: '300px' }}>
                            <InputLabel id="set-traderule-label">Trade rule</InputLabel>
                            <Select labelId="set-traderule-label" id="set-traderule-select" value={tradeRuleId} label="Trade rule"
                                onChange={e => setTradeRuleId(e.target.value)} >
                                {tradeRules.map((tradeRule) => (
                                    <MenuItem key={tradeRule.id} value={tradeRule.id}> {tradeRule.name} </MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                    </div>
                </DialogContent>
                <DialogActions>
                    <div className='dialog-control'>
                        <Button className='m-3' variant="outlined" onClick={closeDialog}>Cancel</Button>
                        <Button className='m-3' variant="contained" onClick={() => {
                            setTradeRule(tradeOrder.id, tradeRuleId);
                            closeDialog();
                        }}>Set trade rule</Button>
                    </div>
                </DialogActions>
            </Dialog>
        </div>
    );
}

export default TradeRuleDialog;