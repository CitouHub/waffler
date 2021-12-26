import React from 'react';
import TextField from '@mui/material/TextField';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';

const StatisticsFilter = ({ updateFilter, filter }) => {
    return (
        <div className="d-flex">
            <div className="m-2">
                <TextField
                    id="filter-fromDate"
                    label="From"
                    type="date"
                    defaultValue={filter?.fromDate?.toJSON()?.slice(0, 10)}
                    sx={{ width: 220 }}
                    InputLabelProps={{
                        shrink: true,
                    }}
                    onChange={e => updateFilter({ ...filter, fromDate: new Date(e.target.value) })}
                />
            </div>
            <div className="m-2">
                <TextField
                    id="filter-toDate"
                    label="To"
                    type="date"
                    defaultValue={filter?.toDate?.toJSON()?.slice(0, 10)}
                    sx={{ width: 220 }}
                    InputLabelProps={{
                        shrink: true,
                    }}
                    onChange={e => updateFilter({ ...filter, toDate: new Date(e.target.value) })}
                />
            </div>
            <div className="m-2">
                <FormControl>
                    <InputLabel id="filter-statisticsMode-label">Include</InputLabel>
                    <Select sx={{ width: 220 }} labelId="filter-statisticsMode-label" id="filter-statisticsMode-select" value={filter.statisticsMode} label="Include"
                        onChange={e => updateFilter({ ...filter, statisticsMode: e.target.value })} >
                        <MenuItem value={1}>Live orders</MenuItem>
                        <MenuItem value={2}>Test orders</MenuItem>
                        <MenuItem value={3}>Both</MenuItem>
                    </Select>
                </FormControl>
            </div>
        </div>
    )
};

export default StatisticsFilter;