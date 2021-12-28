import React, { useState } from 'react';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';

import './dialog.css';

const TradeRuleTestDialog = ({ dialogOpen, setDialogOpen, startTradeRuleTest }) => {
    let fromDate = new Date();
    let toDate = new Date();
    fromDate.setDate(fromDate.getDate() - 90);

    const [tradeRuleTest, setTradeRuleTest] = useState({
        fromDate: fromDate,
        toDate: toDate,
        minuteStep: 15
    });

    return (
        <div>
            <Dialog open={dialogOpen}>
                <DialogTitle>Trade rule test</DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        Select the time span for which to run the test
                    </DialogContentText>
                    <div className='d-flex mt-4'>
                        <div className='mr-2'>
                            <TextField
                                id="trt-fromDate"
                                label="From"
                                type="date"
                                defaultValue={tradeRuleTest?.fromDate.toJSON().slice(0, 10)}
                                sx={{ width: 220 }}
                                InputLabelProps={{
                                    shrink: true,
                                }}
                                onChange={e => setTradeRuleTest({ ...tradeRuleTest, fromDate: new Date(e.target.value) })}
                            />
                        </div>
                        <div>
                            <TextField
                                id="trt-toDate"
                                label="To"
                                type="date"
                                defaultValue={tradeRuleTest?.toDate.toJSON().slice(0, 10)}
                                sx={{ width: 220 }}
                                InputLabelProps={{
                                    shrink: true,
                                }}
                                onChange={e => setTradeRuleTest({ ...tradeRuleTest, toDate: new Date(e.target.value) })}
                            />
                        </div>
                    </div>
                </DialogContent>
                <DialogActions>
                    <div className='dialog-control'>
                        <Button className='m-3' variant="outlined" onClick={() => setDialogOpen(false)}>Cancel</Button>
                        <Button className='m-3' variant="contained" onClick={() => startTradeRuleTest(tradeRuleTest)}>Start test</Button>
                    </div>
                </DialogActions>
            </Dialog>
        </div>
    );
}

export default TradeRuleTestDialog;