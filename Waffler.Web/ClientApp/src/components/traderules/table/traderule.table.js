import React, { useState } from 'react';
import Box from '@mui/material/Box';
import Collapse from '@mui/material/Collapse';
import IconButton from '@mui/material/IconButton';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import TradeRuleForm from '../form/traderule.form';

import './table.css';

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
                            <Typography variant="h6" gutterBottom component="div">
                                Conditions
                            </Typography>
                            <Table size="small" aria-label="purchases">
                                <TableHead>
                                    <TableRow>
                                        <TableCell align="right">Description</TableCell>
                                        <TableCell align="right">Value type</TableCell>
                                        <TableCell align="right">From</TableCell>
                                        <TableCell align="right">From offset</TableCell>
                                        <TableCell align="right">To</TableCell>
                                        <TableCell align="right">To offset</TableCell>
                                        <TableCell align="right">Sample direction</TableCell>
                                        <TableCell align="right">Condition</TableCell>
                                        <TableCell align="right">Change</TableCell>
                                        <TableCell align="right">Active</TableCell>
                                    </TableRow>
                                </TableHead>
                                {/*<TableBody>*/}
                                {/*    {row.tradeRuleConditions.map((tradeRuleConditionRow) => (*/}
                                {/*        <TableRow key={tradeRuleConditionRow.id}>*/}
                                {/*            <TableCell align="right" colSpan={10}>*/}
                                {/*                <TradeRuleForm />*/}
                                {/*            </TableCell>*/}
                                {/*        </TableRow>*/}
                                {/*        //<TableRow key={tradeRuleConditionRow.id}>*/}
                                {/*        //    <TableCell align="right">{tradeRuleConditionRow.description}</TableCell>*/}
                                {/*        //    <TableCell align="right">{tradeRuleConditionRow.candleStickValueTypeName}</TableCell>*/}
                                {/*        //    <TableCell align="right">{tradeRuleConditionRow.fromMinutesSample}</TableCell>*/}
                                {/*        //    <TableCell align="right">{tradeRuleConditionRow.fromMinutesOffset}</TableCell>*/}
                                {/*        //    <TableCell align="right">{tradeRuleConditionRow.toMinutesSample}</TableCell>*/}
                                {/*        //    <TableCell align="right">{tradeRuleConditionRow.toMinutesOffset}</TableCell>*/}
                                {/*        //    <TableCell align="right">{tradeRuleConditionRow.tradeRuleConditionSampleDirectionName}</TableCell>*/}
                                {/*        //    <TableCell align="right">{tradeRuleConditionRow.tradeRuleConditionComparatorName}</TableCell>*/}
                                {/*        //    <TableCell align="right">{tradeRuleConditionRow.deltaPercent}</TableCell>*/}
                                {/*        //    <TableCell align="right">{tradeRuleConditionRow.isActive}</TableCell>*/}
                                {/*        //</TableRow>*/}
                                {/*    ))}*/}
                                {/*</TableBody>*/}
                            </Table>
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
