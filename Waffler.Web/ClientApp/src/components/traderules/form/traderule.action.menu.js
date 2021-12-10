import React, { useState } from 'react';
import { styled, alpha } from '@mui/material/styles';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import SaveIcon from '@mui/icons-material/Save';
import FileDownloadIcon from '@mui/icons-material/FileDownload';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import PlayCircleIcon from '@mui/icons-material/PlayCircle';
import StopCircleIcon from '@mui/icons-material/StopCircle';
import DeleteIcon from '@mui/icons-material/Delete';
import Divider from '@mui/material/Divider';
import IconButton from '@mui/material/IconButton';
import MoreVertIcon from '@mui/icons-material/MoreVert';

const StyledMenu = styled((props) => (
    <Menu
        elevation={0}
        anchorOrigin={{
            vertical: 'bottom',
            horizontal: 'right',
        }}
        transformOrigin={{
            vertical: 'top',
            horizontal: 'right',
        }}
        {...props}
    />
))(({ theme }) => ({
    '& .MuiPaper-root': {
        borderRadius: 6,
        marginTop: theme.spacing(1),
        minWidth: 180,
        color:
            theme.palette.mode === 'light' ? 'rgb(55, 65, 81)' : theme.palette.grey[300],
        boxShadow:
            'rgb(255, 255, 255) 0px 0px 0px 0px, rgba(0, 0, 0, 0.05) 0px 0px 0px 1px, rgba(0, 0, 0, 0.1) 0px 10px 15px -3px, rgba(0, 0, 0, 0.05) 0px 4px 6px -2px',
        '& .MuiMenu-list': {
            padding: '4px 0',
        },
        '& .MuiMenuItem-root': {
            '& .MuiSvgIcon-root': {
                fontSize: 18,
                color: theme.palette.text.secondary,
                marginRight: theme.spacing(1.5),
            },
            '&:active': {
                backgroundColor: alpha(
                    theme.palette.primary.main,
                    theme.palette.action.selectedOpacity,
                ),
            },
        },
    },
}));

const TradeRuleActionMenu = ({ runningTest, saveTradeRule, copyTradeRule, exportTradeRule, deleteTradeRule, startTradeRuleTest, stopTradeRuleTest}) => {
    const [anchorEl, setAnchorEl] = useState(null);
    const open = Boolean(anchorEl);

    return (
        <div>
            <IconButton
                aria-label="trade rule actions"
                id="tr-action-button"
                aria-controls="long-menu"
                aria-expanded={open ? 'true' : undefined}
                aria-haspopup="true"
                onClick={e => setAnchorEl(e.currentTarget)}
            >
                <MoreVertIcon />
            </IconButton>
            <StyledMenu
                id="tr-action-menu"
                MenuListProps={{
                    'aria-labelledby': 'tr-action-button',
                }}
                anchorEl={anchorEl}
                open={open}
                onClose={e => setAnchorEl(null)}
            >
                <MenuItem onClick={e => {
                    saveTradeRule();
                    setAnchorEl(null);
                }} disableRipple>
                    <SaveIcon />
                    Save
                </MenuItem>
                <MenuItem onClick={e => {
                    exportTradeRule();
                    setAnchorEl(null);
                }} disableRipple>
                    <FileDownloadIcon />
                    Export
                </MenuItem>
                <MenuItem onClick={e => {
                    copyTradeRule();
                    setAnchorEl(null);
                }} disableRipple>
                    <ContentCopyIcon />
                    Copy
                </MenuItem>
                <MenuItem onClick={e => {
                    deleteTradeRule();
                    setAnchorEl(null);
                }} disableRipple>
                    <DeleteIcon />
                    Delete
                </MenuItem>
                <Divider sx={{ my: 0.5 }} />
                <MenuItem onClick={e => {
                    startTradeRuleTest();
                    setAnchorEl(null);
                }} disableRipple disabled={runningTest}>
                    <PlayCircleIcon />
                    Start test
                </MenuItem>
                <MenuItem onClick={e => {
                    stopTradeRuleTest();
                    setAnchorEl(null);
                }} disableRipple disabled={!runningTest}>
                    <StopCircleIcon />
                    Stop test
                </MenuItem>
            </StyledMenu>
        </div>
    );
}

export default TradeRuleActionMenu