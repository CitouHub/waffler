import React, { useState } from 'react';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';

import ProfileService from '../../services/profile.service';

import './profile.css';

const ChangePasswordDialog = ({ dialogOpen, setDialogOpen }) => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState();
    const [password, setPassword] = useState({
        oldPassword: '',
        newPassword: '',
        confirmPassword: ''
    });

    const changePassword = () => {
        if (password.newPassword === password.confirmPassword) {
            setLoading(true);
            ProfileService.updatePassword(password).then((result) => {
                if (result === true) {
                    setDialogOpen(false);
                } else {
                    setError('Incorrect password');
                }
                setLoading(false);
            });
        } else {
            setError('Password does not match');
        }
    };

    return (
        <div>
            <Dialog open={dialogOpen}>
                <DialogTitle>Change password</DialogTitle>
                <DialogContent>
                    <div className='mt-4'>
                        <div className='mb-2'>
                            <TextField sx={{ width: '100%' }} id="cpd-old-password" label="Old password" variant="outlined" type="password" value={password.oldPassword}
                                onChange={e => setPassword({ ...password, oldPassword: e.target.value })}
                            />
                        </div>
                        <div className='mb-2'>
                            <TextField sx={{ width: '100%' }} id="cpd-new-password" label="New password" variant="outlined" type="password" value={password.newPassword}
                                onChange={e => setPassword({ ...password, newPassword: e.target.value })}
                            />
                        </div>
                        <div className='mb-2'>
                            <TextField sx={{ width: '100%' }} id="cpd-conf-password" label="Confirm password" variant="outlined" type="password" value={password.confirmPassword}
                                onChange={e => setPassword({ ...password, confirmPassword: e.target.value })}
                            />
                        </div>
                        {error && <span className='error'>{error}</span>}
                    </div>
                </DialogContent>
                <DialogActions>
                    <div className='dialog-control'>
                        <Button className='m-3' variant="outlined" disabled={loading} onClick={() => setDialogOpen(false)}>Cancel</Button>
                        <Button className='m-3' variant="contained" disabled={loading} onClick={() => changePassword()}>Change password</Button>
                    </div>
                </DialogActions>
            </Dialog>
        </div>
    );
}

export default ChangePasswordDialog;