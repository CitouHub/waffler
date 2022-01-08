import React, { useState } from 'react';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import DateSpanFilter from '../../filter/datespan.filter'

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
                        <DateSpanFilter filter={tradeRuleTest} updateFilter={setTradeRuleTest} />
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