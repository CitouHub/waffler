import React, { useState, useEffect } from "react";
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell, { tableCellClasses } from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Paper from '@mui/material/Paper';
import { styled } from '@mui/material/styles';
import { timeFormat } from "d3-time-format";

import LoadingBar from '../utils/loadingbar';
import TradeFilter from '../filter/trade.filter';

import TradeOrderService from '../../services/tradeorder.service';
import TradeRuleService from '../../services/traderule.service';

const TradeOrders = () => {
    let fromDate = new Date();
    let toDate = new Date();
    fromDate.setDate(fromDate.getDate() - 90);

    const [loading, setLoading] = useState(true);
    const [tradeOrders, setTradeOrders] = useState([]);
    const [tradeOrdersDisplay, setTradeOrdersDisplay] = useState([]);
    const [tradeRules, setTradeRules] = useState([]);
    const [selectedTradeRules, setSelectedTradeRules] = useState([]);
    const [tradeOrderStatuses, setTradeOrderStatuses] = useState([]);
    const [selectedTradeOrderStatuses, setSelectedTradeOrderStatuses] = useState([]);
    const [filter, setFilter] = useState({
        fromDate: fromDate,
        toDate: toDate
    });

    useEffect(() => {
        TradeRuleService.getTradeRules().then((result) => {
            result.push({
                id: 0,
                name: 'Manual'
            });
            setTradeRules(result);
            setSelectedTradeRules(result);
        });

        TradeOrderService.getTradeOrderStatuses().then((result) => {
            setTradeOrderStatuses(result);
            setSelectedTradeOrderStatuses(result);
        });
    }, []);

    useEffect(() => {
        updateTradeOrders();
    }, [filter]);

    const updateTradeOrders = () => {
        setLoading(true);
        let toDate = new Date(filter.toDate);
        toDate.setDate(toDate.getDate() + 1);

        TradeOrderService.getTradeOrders(filter.fromDate, toDate).then((tradeOrdersResult) => {
            if (tradeOrdersResult && tradeOrdersResult.length > 0) {
                tradeOrdersResult.forEach((e) => {
                    e.orderDateTime = new Date(e.orderDateTime);
                });
            }

            setTradeOrders(tradeOrdersResult);
            setLoading(false);
        });
    }

    useEffect(() => {
        if (tradeOrders && tradeOrders.length > 0) {
            const tradeOrdersUpdate = [];

            tradeOrders.forEach((tradeOrder) => {
                if (selectedTradeRules.filter(t => t.id === tradeOrder.tradeRuleId).length > 0 &&
                    selectedTradeOrderStatuses.filter(s => s.id === tradeOrder.tradeOrderStatusId).length > 0) {
                    tradeOrdersUpdate.push(tradeOrder);
                }
            });

            setTradeOrdersDisplay(tradeOrdersUpdate);
        }
    }, [selectedTradeRules, selectedTradeOrderStatuses, tradeOrders]);

    const StyledTableCell = styled(TableCell)(({ theme }) => ({
        [`&.${tableCellClasses.head}`]: {
            backgroundColor: theme.palette.common.black,
            color: theme.palette.common.white,
        },
        [`&.${tableCellClasses.body}`]: {
            fontSize: 14,
        },
    }));

    const StyledTableRow = styled(TableRow)(({ theme }) => ({
        '&:nth-of-type(odd)': {
            backgroundColor: '#f7f9fb',
        },
        // hide last border
        '&:last-child td, &:last-child th': {
            border: 0,
        },
    }));

    const dateFormat = timeFormat("%Y-%m-%d %H:%M");

    return (
        <div>
            <LoadingBar active={loading} />
            <div className='mt-3 mb-3'>
                <h4>Orders</h4>
            </div>
            {loading === false &&
                <div>
                    <TradeFilter
                        filter={filter}
                        updateFilter={(filter) => setFilter(filter)}
                        tradeRules={tradeRules}
                        selectedTradeRules={selectedTradeRules}
                        updateSelectedTradeRules={(selectedTradeRules) => setSelectedTradeRules(selectedTradeRules)}
                        tradeOrderStatuses={tradeOrderStatuses}
                        selectedTradeOrderStatuses={selectedTradeOrderStatuses}
                        updateSelectedTradeStatuses={(selectedTradeOrderStatuses) => setSelectedTradeOrderStatuses(selectedTradeOrderStatuses)}
                    />
                </div>
            }
            {tradeOrders && tradeOrdersDisplay.length > 0 && <div className='mt-4'>
                <TableContainer component={Paper}>
                    <Table sx={{ minWidth: 650 }} size="small" aria-label="a dense table">
                        <TableHead>
                            <TableRow>
                                <StyledTableCell>Order date</StyledTableCell>
                                <StyledTableCell>Trade rule</StyledTableCell>
                                <StyledTableCell>Action</StyledTableCell>
                                <StyledTableCell>Status</StyledTableCell>
                                <StyledTableCell>Price</StyledTableCell>
                                <StyledTableCell>Amount</StyledTableCell>
                                <StyledTableCell>Total value</StyledTableCell>
                                <StyledTableCell>Filled %</StyledTableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {tradeOrdersDisplay.map((tradeOrder) => (
                                <StyledTableRow
                                    key={tradeOrder.id}
                                    sx={{ '&:last-child td, &:last-child th': { border: 0 } }}
                                >
                                    <StyledTableCell component="th" scope="tradeOrder">
                                        {dateFormat(tradeOrder.orderDateTime)}
                                    </StyledTableCell>
                                    <StyledTableCell>{tradeOrder.tradeRuleName}</StyledTableCell>
                                    <StyledTableCell>{tradeOrder.tradeActionName}</StyledTableCell>
                                    <StyledTableCell>{tradeOrder.tradeOrderStatusName}</StyledTableCell>
                                    <StyledTableCell>{tradeOrder.price}</StyledTableCell>
                                    <StyledTableCell>{tradeOrder.amount}</StyledTableCell>
                                    <StyledTableCell>{tradeOrder.totalValue}</StyledTableCell>
                                    <StyledTableCell>{tradeOrder.filledPercent} %</StyledTableCell>
                                </StyledTableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            </div>}
            {tradeOrders && tradeOrdersDisplay.length == 0 && <div className='mt-4'>
                <h5 className="text-center">No orders found</h5>
            </div>}
        </div>
    )
};

export default TradeOrders;
