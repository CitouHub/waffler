import * as React from 'react';
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

const names = [
    'Oliver Hansen',
    'Van Henry',
    'April Tucker',
    'Ralph Hubbard',
    'Omar Alexander',
    'Carlos Abbott',
    'Miriam Wagner',
    'Bradley Wilkerson',
    'Virginia Andrews',
    'Kelly Snyder',
];

export default function MultipleSelect({ tradeRules, updateSelectedTradeRules }) {
    const [selectedTradeRules, setSelectedTradeRules] = React.useState([]);

    const handleChange = (event) => {
        const {
            target: { value },
        } = event;
        setSelectedTradeRules(
            // On autofill we get a the stringified value.
            typeof value === 'string' ? value.split(',') : value,
        );
        updateSelectedTradeRules(selectedTradeRules);
    };

    if (tradeRules !== undefined && tradeRules.length > 0) {
        return (
            <div>
                <FormControl sx={{ width: 300 }}>
                    <InputLabel id="demo-multiple-name-label">Trade rules</InputLabel>
                    <Select
                        labelId="demo-multiple-name-label"
                        id="demo-multiple-name"
                        multiple
                        value={selectedTradeRules}
                        onChange={handleChange}
                        input={<OutlinedInput label="Trade rules" />}
                        MenuProps={MenuProps}
                    >
                        {tradeRules.map((tradeRule) => (
                            <MenuItem
                                key={tradeRule.id}
                                value={tradeRule.id}
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