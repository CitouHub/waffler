import React, { useState, useEffect } from 'react';
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
import ProgressBar from '../../../components/utils/progressbar';
import TradeRuleCondition from '../traderuleconditions';

import TradeRuleService from '../../../services/traderule.service';

let unmount = false;

function Row({ row, tradeRuleAttributes, updateTradeRules }) {
    const [open, setOpen] = useState(false);
    const [traderRuleTestStatus, setTraderRuleTestStatus] = useState({});
    const [runningTest, setRunningTest] = useState(true);

    useEffect(() => {
        getTradeRuleTestStatus(row.id);
    }, [runningTest]);

    useEffect(() => {
        return () => {
            unmount = true;
        }
    }, []);

    const getTradeRuleTestStatus = (tradeRuleId) => {
        if (runningTest === true) {
            TradeRuleService.getTradeRuleTestStatus(tradeRuleId).then((result) => {
                if (result !== undefined && unmount === false) {
                    setTraderRuleTestStatus(result);

                    if (result.progress < 100 && result.aborted === false) {
                        setRunningTest(true);
                        setTimeout(() => getTradeRuleTestStatus(tradeRuleId), 800);
                    } else {
                        setRunningTest(false);
                        setTraderRuleTestStatus({});
                    }
                } else {
                    setRunningTest(false);
                }
            });
        }
    }

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
                    <TradeRuleForm
                        data={row}
                        tradeRuleAttributes={tradeRuleAttributes}
                        updateTradeRules={updateTradeRules}
                        setRunningTest={(runningTest) => setRunningTest(runningTest)}
                        runningTest={runningTest}
                    />
                    <ProgressBar progress={traderRuleTestStatus?.progress ?? 0} />
                </TableCell>
            </TableRow>
            <TableRow>
                <TableCell style={{ paddingBottom: 0, paddingTop: 0 }} colSpan={10}>
                    <Collapse in={open} timeout="auto" unmountOnExit>
                        <Box sx={{ margin: 1 }}>
                            <TradeRuleCondition tradeRuleId={row.id} runningTest={runningTest}/>
                        </Box>
                    </Collapse>
                </TableCell>
            </TableRow>
        </React.Fragment>
    );
}

export default function TradeRuleTable({ tradeRules, tradeRuleAttributes, updateTradeRules }) {
    unmount = false;
    return (
        <TableContainer component={Paper}>
            <Table aria-label="collapsible table">
                <TableBody>
                    {tradeRules.map((tradeRule) => (
                        <Row key={tradeRule.id} row={tradeRule} tradeRuleAttributes={tradeRuleAttributes} updateTradeRules={updateTradeRules} />
                    ))}
                </TableBody>
            </Table>
        </TableContainer>
    );
}
