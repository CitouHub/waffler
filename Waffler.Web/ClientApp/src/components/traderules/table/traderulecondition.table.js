import React from 'react';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import TradeRuleConditionForm from '../form/traderulecondition.form';

import '../table.css';

function Row({ row, tradeRuleConditionAttributes, updateTradeRuleConditions }) {
    return (
        <React.Fragment>
            <TableRow sx={{ '& > *': { borderBottom: 'unset' } }}>
                <TableCell align="right">
                    <TradeRuleConditionForm data={row} tradeRuleConditionAttributes={tradeRuleConditionAttributes} updateTradeRuleConditions={updateTradeRuleConditions} />
                </TableCell>
            </TableRow>
        </React.Fragment>
    );
}

export default function TradeRuleConditionTable({ tradeRuleConditions, tradeRuleConditionAttributes, updateTradeRuleConditions }) {
    return (
        <div className='trade-rule-condition mb-4'>
            <TableContainer component={Paper}>
                <Table aria-label="collapsible table">
                    <TableBody>
                        {tradeRuleConditions.map((tradeRuleCondition) => (
                            <Row key={tradeRuleCondition.id} row={tradeRuleCondition} tradeRuleConditionAttributes={tradeRuleConditionAttributes} updateTradeRuleConditions={updateTradeRuleConditions} />
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
        </div>
    );
}
