import * as React from 'react';
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

function Row(props) {
    const { row } = props;
    const [open, setOpen] = React.useState(false);

    return (
        <React.Fragment>
            <TableRow sx={{ '& > *': { borderBottom: 'unset' } }}>
                <TableCell>
                    <IconButton
                        aria-label="expand row"
                        size="small"
                        onClick={() => setOpen(!open)}
                    >
                        {open ? <KeyboardArrowUpIcon /> : <KeyboardArrowDownIcon />}
                    </IconButton>
                </TableCell>
                <TableCell align="right">{row.tradeActionName}</TableCell>
                <TableCell align="right">{row.tradeTypeName}</TableCell>
                <TableCell align="right">{row.tradeConditionOperatorName}</TableCell>
                <TableCell align="right">{row.name}</TableCell>
                <TableCell align="right">{row.description}</TableCell>
                <TableCell align="right">{row.amount}</TableCell>
                <TableCell align="right">{row.tradeMinIntervalMinutes}</TableCell>
                <TableCell align="right">{row.lastTrigger}</TableCell>
                <TableCell align="right">{row.isActive}</TableCell>
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
                                <TableBody>
                                    {row.tradeRuleConditions.map((tradeRuleConditionRow) => (
                                        <TableRow key={tradeRuleConditionRow.id}>
                                            <TableCell align="right" colSpan={10}>
                                                <TradeRuleForm />
                                            </TableCell>
                                        </TableRow>
                                        //<TableRow key={tradeRuleConditionRow.id}>
                                        //    <TableCell align="right">{tradeRuleConditionRow.description}</TableCell>
                                        //    <TableCell align="right">{tradeRuleConditionRow.candleStickValueTypeName}</TableCell>
                                        //    <TableCell align="right">{tradeRuleConditionRow.fromMinutesSample}</TableCell>
                                        //    <TableCell align="right">{tradeRuleConditionRow.fromMinutesOffset}</TableCell>
                                        //    <TableCell align="right">{tradeRuleConditionRow.toMinutesSample}</TableCell>
                                        //    <TableCell align="right">{tradeRuleConditionRow.toMinutesOffset}</TableCell>
                                        //    <TableCell align="right">{tradeRuleConditionRow.tradeRuleConditionSampleDirectionName}</TableCell>
                                        //    <TableCell align="right">{tradeRuleConditionRow.tradeRuleConditionComparatorName}</TableCell>
                                        //    <TableCell align="right">{tradeRuleConditionRow.deltaPercent}</TableCell>
                                        //    <TableCell align="right">{tradeRuleConditionRow.isActive}</TableCell>
                                        //</TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </Box>
                    </Collapse>
                </TableCell>
            </TableRow>
        </React.Fragment>
    );
}

export default function TradeRuleTable(props) {
    return (
        <TableContainer component={Paper}>
            <Table aria-label="collapsible table">
                <TableHead>
                    <TableRow>
                        <TableCell />
                        <TableCell align="right">Action</TableCell>
                        <TableCell align="right">Type</TableCell>
                        <TableCell align="right">Condition operator</TableCell>
                        <TableCell align="right">Name</TableCell>
                        <TableCell align="right">Description</TableCell>
                        <TableCell align="right">Amount</TableCell>
                        <TableCell align="right">Min trade interval</TableCell>
                        <TableCell align="right">Last trade</TableCell>
                        <TableCell align="right">Active</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {props.data.map((tradeRule) => (
                        <Row key={tradeRule.id} row={tradeRule} />
                    ))}
                </TableBody>
            </Table>
        </TableContainer>
    );
}
