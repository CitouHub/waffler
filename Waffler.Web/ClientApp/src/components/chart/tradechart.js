import React, { useState, useEffect, useRef } from "react";
import {
    ema,
    discontinuousTimeScaleProviderBuilder,
    CandlestickSeries,
    Chart,
    ChartCanvas,
    HoverTooltip,
    XAxis,
    YAxis,
} from "react-financial-charts";
import { useWindowSize } from "../../util/use.windowsize";

import LoadingBar from '../utils/loadingbar'
import BuyNewAnnotate from './annotate/buy.new.annotate'
import BuyPartialAnnotate from './annotate/buy.partial.annotate'
import BuyCompleteAnnotate from './annotate/buy.complete.annotate'
import BuyTestAnnotate from './annotate/buy.test.annotate'
import SellNewAnnotate from './annotate/sell.new.annotate'
import SellPartialAnnotate from './annotate/sell.partial.annotate'
import SellCompleteAnnotate from './annotate/sell.complete.annotate'
import SellTestAnnotate from './annotate/sell.test.annotate'
import CandleStickService from '../../services/candlestick.service'
import TradeOrderService from '../../services/tradeorder.service'
import ToolTipHelper from './tooltip/hover.tooltip'
import TradeFilter from '../filter/trade.filter'
import SyncBar from './syncbar'

import './chart.css';

const TradeChart = () => {
    let syncActive = true;
    const [loading, setLoading] = useState(true);
    const [dimensions, setDimensions] = useState({ width: 0, height: 0 });
    const [syncStatus, setSyncStatus] = useState({});
    const [candleSticks, setCandleSticks] = useState([]);
    const [candleSticksChart, setCandleSticksChart] = useState([]);
    const [tradeOrders, setTradeOrders] = useState([]);

    const canvasRef = useRef();
    const windowSize = useWindowSize();

    useEffect(() => {
        if (canvasRef.current) {
            setDimensions({
                width: canvasRef.current.offsetWidth,
                height: canvasRef.current.offsetHeight
            });
        }
    }, [windowSize]);

    useEffect(() => {
        getCandleStickSyncStatus();

        return () => {
            syncActive = false;
        }
    }, []);

    useEffect(() => {
        if (loading === false) {
            getCandleStickSyncStatus();
        }
    }, [loading]);

    useEffect(() => {
        if (canvasRef.current) {
            setDimensions({
                width: canvasRef.current.offsetWidth,
                height: canvasRef.current.offsetHeight
            });
        }
    }, [candleSticksChart]);

    const datesChanged = (filter) => {
        updateCandleStickData(filter);
    }

    const updateCandleStickData = (filter) => {
        if (syncStatus.finished && filter.fromDate && filter.toDate) {
            setLoading(true);
            let toDateExtra = new Date(filter.toDate);
            toDateExtra.setDate(toDateExtra.getDate() + 1);

            var getCandleSticks = CandleStickService.getCandleSticks(filter.fromDate, toDateExtra, 1, 30);
            var getTradeOrders = TradeOrderService.getTradeOrders(filter.fromDate, toDateExtra);

            Promise.all([getCandleSticks, getTradeOrders]).then((result) => {
                if (result[0] && result[0].length > 0) {
                    result[0].forEach((e) => {
                        e.date = new Date(e.date);
                    });
                }

                if (result[0] && result[0].length > 0) {
                    result[1].forEach((tradeOrder) => {
                        tradeOrder.orderDateTime = new Date(tradeOrder.orderDateTime);
                    });
                }

                setCandleSticks(result[0]);
                setTradeOrders(result[1]);
                updateCandleSticksChart(result[0], result[1], filter.selectedTradeRules, filter.selectedTradeOrderStatuses);

                setLoading(false);
            });
        }
    }

    const selectionsChanged = (filter) => {
        updateCandleSticksChart(candleSticks, tradeOrders, filter.selectedTradeRules, filter.selectedTradeOrderStatuses)
    }

    const updateCandleSticksChart = (candleSticksData, tradeOrderData, selectedTradeRules, selectedTradeOrderStatuses) => {
        if (candleSticksData && tradeOrderData && selectedTradeRules && selectedTradeOrderStatuses &&
            candleSticksData.length > 0 && tradeOrderData.length > 0) {

            const candleSticksUpdate = [...candleSticksData];
            tradeOrderData.forEach((tradeOrder) => {
                let candleStick = candleSticksUpdate.find((candleStick) => {
                    if (candleStick.date >= tradeOrder.orderDateTime) {
                        return candleStick;
                    }

                    return null;
                });

                if (candleStick === undefined) {
                    candleStick = candleSticksUpdate[candleSticksUpdate.length - 1];
                }

                if (selectedTradeRules.filter(t => t.id === tradeOrder.tradeRuleId).length > 0 &&
                    selectedTradeOrderStatuses.filter(s => s.id === tradeOrder.tradeOrderStatusId).length > 0) {
                    candleStick.tradeOrder = tradeOrder;
                }
                else {
                    candleStick.tradeOrder = undefined;
                }
            });

            setCandleSticksChart(candleSticksUpdate);
        }
    }

    const getCandleStickSyncStatus = () => {
        CandleStickService.getCandleSticksSyncStatus().then((syncStatus) => {
            if (syncStatus) {
                syncStatus.firstPeriodDateTime = new Date(syncStatus.firstPeriodDateTime);
                syncStatus.lastPeriodDateTime = new Date(syncStatus.lastPeriodDateTime);

                if (syncActive) {
                    setSyncStatus(syncStatus);
                }

                if (syncStatus.finished) {
                    syncActive = false;
                }
            }

            if (syncActive) {
                setTimeout(() => getCandleStickSyncStatus(), 1500);
            }
        });
    }

    const margin = { left: 0, right: 48, top: 24, bottom: 24 };
    const xScaleProvider = discontinuousTimeScaleProviderBuilder().inputDateAccessor(
        d => d.date,
    );

    const ema12 = ema()
        .id(1)
        .options({ windowSize: 12 })
        .merge((d, c) => {
            d.ema12 = c;
        })
        .accessor((d) => d.ema12);

    const calculatedData = ema12(candleSticksChart);

    const { data, xScale, xAccessor, displayXAccessor } = xScaleProvider(calculatedData);

    const max = xAccessor(data[data.length - 1]);
    const min = xAccessor(data[Math.max(0, data.length - 100)]);
    const xExtents = [min, max];
    const yExtents = (data) => {
        return [data.high, data.low];
    };

    return (
        <div className='chart-wrapper' ref={canvasRef}>
            <LoadingBar active={loading && syncStatus?.finished} />
            <div className='mt-3 mb-3'>
                <h4>Chart</h4>
            </div>
            {!syncStatus?.finished && <SyncBar currentDate={syncStatus?.lastPeriodDateTime?.toJSON()?.slice(0, 10)} progress={syncStatus.progress} throttled={syncStatus.isThrottled} />}
            {syncStatus?.finished && dimensions.width > 0 && dimensions.height > 0 &&
                <div>
                    <TradeFilter
                        datesChanged={datesChanged}
                        selectionsChanged={selectionsChanged}
                    />
                    {candleSticksChart && candleSticksChart.length > 0 && <ChartCanvas
                        height={dimensions.height - 150}
                        ratio={1}
                        width={dimensions.width}
                        margin={margin}
                        data={data}
                        displayXAccessor={displayXAccessor}
                        seriesName="BTC_EUR"
                        xScale={xScale}
                        xAccessor={xAccessor}
                        xExtents={xExtents}
                    >
                        <Chart id={1} yExtents={yExtents}>
                            <XAxis showGridLines />
                            <YAxis showGridLines />
                            <CandlestickSeries />
                            <BuyNewAnnotate />
                            <BuyPartialAnnotate />
                            <BuyCompleteAnnotate />
                            <BuyTestAnnotate />
                            <SellNewAnnotate />
                            <SellPartialAnnotate />
                            <SellCompleteAnnotate />
                            <SellTestAnnotate />
                            <HoverTooltip
                                yAccessor={ema12.accessor()}
                                tooltip={{ content: ({ currentItem, xAccessor }) => ToolTipHelper.getToolTip(currentItem, xAccessor) }}
                            />
                        </Chart>
                    </ChartCanvas>}
                </div>
            }
        </div>
    );
};

export default TradeChart;
