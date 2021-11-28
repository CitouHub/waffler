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

export default function MultipleSelect({ tradeRules, selectedTradeRules, updateSelectedTradeRules }) {
    if (tradeRules !== undefined && tradeRules.length > 0) {
        return (
            <div>
                <FormControl sx={{ width: 300 }}>
                    <InputLabel id="traderule-multiselect-label">Trade rules</InputLabel>
                    <Select
                        labelId="traderule-multiselect-label"
                        id="traderule-multiselect-select"
                        multiple
                        value={selectedTradeRules}
                        onChange={e => updateSelectedTradeRules(e.target.value)}
                        input={<OutlinedInput label="Trade rules" />}
                        MenuProps={MenuProps}
                    >
                        {tradeRules.map((tradeRule) => (
                            <MenuItem
                                key={tradeRule.id}
                                value={tradeRule}
                            >
                                {tradeRule.name}
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