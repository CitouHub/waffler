import React from 'react';
import AdapterDateFns from '@mui/lab/AdapterDateFns';
import LocalizationProvider from '@mui/lab/LocalizationProvider';
import TextField from '@mui/material/TextField';
import DatePicker from '@mui/lab/DatePicker';

const DateSpanFilter = ({ updateFilter, filter }) => {

    return (
        <React.Fragment>
            <LocalizationProvider dateAdapter={AdapterDateFns}>
                <div className="mr-2 mt-2 mb-2">
                    <DatePicker
                        id="datespan-filter-fromDate"
                        label="From"
                        value={filter?.fromDate}
                        maxDate={filter?.toDate}
                        onChange={newDate => updateFilter({ ...filter, fromDate: newDate })}
                        mask="____-__-__"
                        inputFormat="yyyy-MM-dd"
                        renderInput={(params) => <TextField {...params} />}
                    />
                </div>
                <div className="mt-2 mb-2">
                    <DatePicker
                        id="datespan-filter-toDate"
                        label="To"
                        value={filter?.toDate}
                        minDate={filter?.fromDate}
                        maxDate={new Date()}
                        onChange={newDate => updateFilter({ ...filter, toDate: newDate })}
                        mask="____-__-__"
                        inputFormat="yyyy-MM-dd"
                        renderInput={(params) => <TextField {...params} />}
                    />
                </div>
            </LocalizationProvider>
        </React.Fragment>
    )
};

export default DateSpanFilter;