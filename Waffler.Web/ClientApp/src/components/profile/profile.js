import React, { useState, useEffect } from "react";
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import LoadingBar from '../../components/utils/loadingbar';
import ChangePasswordDialog from '../utils/dialog/changepassword.dialog'

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faStepBackward } from "@fortawesome/free-solid-svg-icons";

import ProfileService from '../../services/profile.service';
import CandleStickService from '../../services/candlestick.service';

import './profile.css';

const Profile = () => {
    const [loading, setLoading] = useState(true);
    const [dialogOpen, setDialogOpen] = useState(false);
    const [profile, setProfile] = useState({});

    useEffect(() => {
        ProfileService.getProfile().then((profile) => {
            profile.candleStickSyncFromDate = new Date(profile.candleStickSyncFromDate);
            setProfile(profile);
            setLoading(false);
        });
    }, []);

    const saveProfile = () => {
        setLoading(true);
        ProfileService.updateProfile(profile).then(() => {
            setLoading(false);
        });
    };

    const resetSync = () => {
        setLoading(true);
        ProfileService.updateProfile(profile).then((success) => {
            if (success) {
                CandleStickService.resetCandleSticksSync().then(() => {
                    setLoading(false);
                });
            }
        });
    }

    return (
        <div>
            <LoadingBar active={loading} />
            {!loading &&
                <div className='mt-3 mb-3'>
                    <h4>Profile</h4>
                    <div className="mt-3 mb-3">
                        <div>
                            <TextField sx={{ width: '100%' }} id="api-key-textfield" label="Api key" variant="outlined" type="text" value={profile.apiKey} multiline rows={6}
                                onChange={e => setProfile({ ...profile, apiKey: e.target.value })} />
                        </div>
                        <div className='get-api-key'>
                            <a href="https://exchange.bitpanda.com/account/keys" target="_blank">Get you Api key here</a>
                        </div>
                    </div>
                    <div className="mt-3 mb-3 sync-control">
                        <TextField
                            id="candlestick-sync-fromDate"
                            label="Sync data from"
                            type="date"
                            defaultValue={profile?.candleStickSyncFromDate?.toJSON()?.slice(0, 10)}
                            sx={{ width: '25%' }}
                            InputLabelProps={{
                                shrink: true,
                            }}
                            onChange={e => setProfile({ ...profile, candleStickSyncFromDate: new Date(e.target.value) })}
                        />
                        <div className='stepback-center'>
                            <span className='fa-icon' onClick={resetSync}><FontAwesomeIcon icon={faStepBackward} className="mr-2" /></span>
                        </div>
                    </div>
                    <div>
                        <Button className='mr-2' variant="contained" onClick={saveProfile} disabled={loading}>Save changes</Button>
                        <Button className='mr-2' variant="contained" onClick={() => setDialogOpen(true)} disabled={loading}>Change password</Button>
                    </div>
                </div>
            }
            <ChangePasswordDialog dialogOpen={dialogOpen} setDialogOpen={setDialogOpen} />
        </div>
    )
};

export default Profile;
