import React, { useState } from 'react';
import Box from '@mui/material/Box';
import Collapse from '@mui/material/Collapse';
import IconButton from '@mui/material/IconButton';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import TradeRuleForm from '../form/traderule.form';

import TradeRuleCondition from '../traderuleconditions';

function Row({ row, tradeRuleAttributes, updateTradeRules}) {
    const [open, setOpen] = useState(false);

    return (
        <React.Fragment>
            <TableRow sx={{ '& > *': { borderBottom: 'unset' } }}>
                <TableCell width="2%">
                    <IconButton
                        aria-label="expand row"
                        size="small"
                        onClick={() => setOpen(!open)}
                    >
                        {open ? <KeyboardArrowUpIcon /> : <KeyboardArrowDownIcon />}
                    </IconButton>
                </TableCell>
                <TableCell width="98%" colSpan={9} align="right">
                    <TradeRuleForm data={row} tradeRuleAttributes={tradeRuleAttributes} updateTradeRules={updateTradeRules} />
                </TableCell>
            </TableRow>
            <TableRow>
                <TableCell style={{ paddingBottom: 0, paddingTop: 0 }} colSpan={10}>
                    <Collapse in={open} timeout="auto" unmountOnExit>
                        <Box sx={{ margin: 1 }}>
                            <TradeRuleCondition tradeRuleId={row.id}/>
                        </Box>
                    </Collapse>
                </TableCell>
            </TableRow>
        </React.Fragment>
    );
}

export default function TradeRuleTable({ tradeRules, tradeRuleAttributes, updateTradeRules }) {
    return (
        <TableContainer component={Paper}>
            <Table aria-label="collapsible table">
                <TableBody>
                    {tradeRules.map((tradeRule) => (
                        <Row key={tradeRule.id} row={tradeRule} tradeRuleAttributes={tradeRuleAttributes} updateTradeRules={updateTradeRules}/>
                    ))}
                </TableBody>
            </Table>
        </TableContainer>
    );
}
