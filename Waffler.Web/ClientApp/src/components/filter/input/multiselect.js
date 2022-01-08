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

const MultiSelect = ({ items, selectedItems, updateSelectedItems, label }) => {
    if (items !== undefined && items.length > 0) {
        return (
            <div className="ml-4 mt-2 mb-2">
                <FormControl sx={{ width: 300 }}>
                    <InputLabel id="multiselect-label">{label}</InputLabel>
                    <Select
                        labelId="multiselect-label"
                        id="multiselect-select"
                        multiple
                        value={selectedItems}
                        onChange={e => updateSelectedItems(e.target.value)}
                        input={<OutlinedInput label={label} />}
                        MenuProps={MenuProps}
                    >
                        {items.map((item) => (
                            <MenuItem key={item.id} value={item}>{item.name}</MenuItem>
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

export default MultiSelect;