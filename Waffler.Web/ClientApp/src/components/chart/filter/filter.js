import React from 'react';
import TextField from '@mui/material/TextField';
import MultiSelect from './multiselect';

import './filter.css';

const ChartFilter = ({ updateFilter, filter }) => {
    return (
        <div className="filter-wrapper">
            <div className="filter-control">
                <TextField
                    id="fromDate"
                    label="From"
                    type="date"
                    defaultValue={filter?.fromDate.toJSON().slice(0, 10)}
                    sx={{ width: 220 }}
                    InputLabelProps={{
                        shrink: true,
                    }}
                    onChange={e => updateFilter({ ...filter, fromDate: new Date(e.target.value) })}
                />
            </div>
            <div className="filter-control">
                <TextField
                    id="toDate"
                    label="To"
                    type="date"
                    defaultValue={filter?.toDate.toJSON().slice(0, 10)}
                    sx={{ width: 220 }}
                    InputLabelProps={{
                        shrink: true,
                    }}
                    onChange={e => updateFilter({ ...filter, toDate: new Date(e.target.value) })}
                />
            </div>
            <div className="filter-control">
                <MultiSelect />
            </div>
        </div>
    )
};

export default ChartFilter;
