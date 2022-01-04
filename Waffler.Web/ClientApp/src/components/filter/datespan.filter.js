import React from 'react';
import TextField from '@mui/material/TextField';

const DateSpanFilter = ({ updateFilter, filter }) => {
    return (
        <React.Fragment>
            <div className="mr-4 mt-2 mb-2">
                <TextField
                    id="datespan-filter-fromDate"
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
            <div className="mr-4 mt-2 mb-2">
                <TextField
                    id="datespan-filter-toDate"
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
        </React.Fragment>
    )
};

export default DateSpanFilter;