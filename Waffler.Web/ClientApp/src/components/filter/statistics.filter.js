import React from 'react';
import FormControl from '@mui/material/FormControl';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import DateSpanFilter from './datespan.filter';

const StatisticsFilter = ({ updateFilter, filter }) => {
    return (
        <div className="d-flex">
            <DateSpanFilter filter={filter} updateFilter={updateFilter} />
            <div className="ml-4 mr-4 mt-2 mb-2">
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