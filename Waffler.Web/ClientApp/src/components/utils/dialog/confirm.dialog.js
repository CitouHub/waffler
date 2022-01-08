import React from 'react';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';

import './dialog.css';

const ConfirmDialog = ({ dialogOpen, setDialogOpen, information, message, confirm }) => {
    return (
        <div>
            <Dialog open={dialogOpen}>
                <DialogTitle>Are you sure?</DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        {information}
                        <br />
                        {message}
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <div className='dialog-control'>
                        <Button className='m-3' variant="outlined" onClick={() => setDialogOpen(false)}>Cancel</Button>
                        <Button className='m-3' variant="contained" onClick={() => confirm()}>Ok</Button>
                    </div>
                </DialogActions>
            </Dialog>
        </div>
    );
}

export default ConfirmDialog;