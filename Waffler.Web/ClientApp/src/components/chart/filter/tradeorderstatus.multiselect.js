import React from 'react';
import OutlinedInput from '@mui/material/OutlinedInput';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';

const ITEM_HEIGHT = 48;
const ITEM_PADDING_TOP = 8;
const MenuProps = {
    PaperProps: {
        style: {
            maxHeight: ITEM_HEIGHT * 4.5 + ITEM_PADDING_TOP,
            width: 250,
        },
    },
};

const TradeOrderStatusMultiSelect = ({ tradeOrderStatuses, selectedTradeOrderStatuses, updateSelectedTradeStatuses }) => {
    if (tradeOrderStatuses !== undefined && tradeOrderStatuses.length > 0) {
        return (
            <div>
                <FormControl sx={{ width: 300 }}>
                    <InputLabel id="tradeorderstatus-multiselect-label">Trade order statuses</InputLabel>
                    <Select
                        labelId="tradeorderstatus-multiselect-label"
                        id="tradeorderstatus-multiselect-select"
                        multiple
                        value={selectedTradeOrderStatuses}
                        onChange={e => updateSelectedTradeStatuses(e.target.value)}
                        input={<OutlinedInput label="Trade order statuses" />}
                        MenuProps={MenuProps}
                    >
                        {tradeOrderStatuses.map((tradeOrderStatus) => (
                            <MenuItem
                                key={tradeOrderStatus.id}
                                value={tradeOrderStatus}
                            >
                                {tradeOrderStatus.name}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
            </div>
        );
    }
    else {
        return null
    }
}

export default TradeOrderStatusMultiSelect;