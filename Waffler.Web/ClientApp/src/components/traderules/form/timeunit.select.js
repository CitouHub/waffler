import React from 'react';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';

const TimeUnitSelect = ({ label, width, onChange, timeUnit }) => {
    return (<FormControl sx={{ width: width }}>
        <InputLabel id="timeUnit-label">{label}</InputLabel>
        <Select labelId="timeUnit-label" id="timeUnit-select" value={timeUnit} label={label}
            onChange={e => onChange(e)} >
            <MenuItem value={1}>Minute</MenuItem>
            <MenuItem value={2}>Hour</MenuItem>
            <MenuItem value={3}>Day</MenuItem>
            <MenuItem value={4}>Week</MenuItem>
        </Select>
    </FormControl>)
}

export default TimeUnitSelect;