import React, { useState, useEffect } from "react";
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

import { useWindowSize } from '../../util/use.windowsize'
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
import TradeRuleService from '../../services/traderule.service'
import ToolTipHelper from './tooltip/hover.tooltip'
import ChartFilter from './filter/filter'
import SyncBar from './syncbar'

import './chart.css';

const TradeChart = () => {
    let fromDate = new Date();
    let toDate = new Date();
    fromDate.setDate(fromDate.getDate() - 30);

    const [loading, setLoading] = useState(true);
    const [syncStatus, setSyncStatus] = useState({});
    const [candleSticks, setCandleSticks] = useState([]);
    const [candleSticksChart, setCandleSticksChart] = useState([]);
    const [tradeOrders, setTradeOrders] = useState([]);
    const [tradeRules, setTradeRules] = useState([]);
    const [selectedTradeRules, setSelectedTradeRules] = useState([]);
    const [filter, setFilter] = useState({
        fromDate: fromDate,
        toDate: toDate
    });

    const windowSize = useWindowSize();

    useEffect(() => {
        getCandleStickSyncStatus();

        TradeRuleService.getTradeRules().then((result) => {
            setTradeRules(result);
            setSelectedTradeRules(result);
        });
    }, []);

    useEffect(() => {
        updateCandleStickData();
    }, [filter]);

    useEffect(() => {
        if (candleSticks.length === 0) {
            updateCandleStickData();
        }
    }, [syncStatus]);

    useEffect(() => {
        const candleSticksUpdate = [...candleSticks];
        if (candleSticksUpdate.length > 0 && tradeOrders.length > 0) {
            tradeOrders.forEach((tradeOrder) => {
                let candleStick = candleSticksUpdate.find((candleStick) => {
                    if (candleStick.date >= tradeOrder.orderDateTime) {
                        return candleStick;
                    }

                    return null;
                });

                if (candleStick === undefined) {
                    candleStick = candleSticksUpdate[candleSticksUpdate.length - 1];
                }

                if (selectedTradeRules.filter(t => t.id === tradeOrder.tradeRuleId).length > 0) {
                    candleStick.tradeOrder = tradeOrder;
                }
                else {
                    candleStick.tradeOrder = undefined;
                }
            });
        }

        setCandleSticksChart(candleSticksUpdate);
    }, [selectedTradeRules, tradeOrders, candleSticks]);

    const getCandleStickSyncStatus = () => {
        CandleStickService.getCandleSticksSyncStatus().then((syncStatus) => {
            syncStatus.firstPeriodDateTime = new Date(syncStatus.firstPeriodDateTime);
            syncStatus.lastPeriodDateTime = new Date(syncStatus.lastPeriodDateTime);

            setSyncStatus(syncStatus);
            if (syncStatus.progress < 95) {
                setTimeout(() => getCandleStickSyncStatus(), 800);
            }
        });
    }

    const updateCandleStickData = () => {
        if (syncStatus.progress >= 95) {
            setLoading(true);
            let toDate = new Date(filter.toDate);
            toDate.setDate(toDate.getDate() + 1);

            CandleStickService.getCandleSticks(filter.fromDate, toDate, 1, 30).then((candleSticksResult) => {
                candleSticksResult.forEach((e) => {
                    e.date = new Date(e.date);
                });

                TradeOrderService.getTradeOrders(filter.fromDate, toDate).then((tradeOrdersResult) => {
                    tradeOrdersResult.forEach((tradeOrder) => {
                        tradeOrder.orderDateTime = new Date(tradeOrder.orderDateTime);
                    });
                    setTradeOrders(tradeOrdersResult);
                    setCandleSticks(candleSticksResult);

                    setLoading(false);
                });
            });
        }
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
        <div>
            {syncStatus.progress >= 95 && <div>
                <LoadingBar active={loading} />
            </div>}
            {syncStatus.progress < 95 && <SyncBar currentDate={syncStatus?.lastPeriodDateTime?.toJSON()?.slice(0, 10)} progress={syncStatus.progress} />}
            {syncStatus.progress >= 95 &&
                <div>
                    <ChartFilter
                        filter={filter}
                        tradeRules={tradeRules}
                        selectedTradeRules={selectedTradeRules}
                        updateSelectedTradeRules={(selectedTradeRules) => setSelectedTradeRules(selectedTradeRules)}
                        updateFilter={(filter) => setFilter(filter)} />
                    {candleSticksChart.length > 0 && <ChartCanvas
                        height={windowSize.height - 200}
                        ratio={1}
                        width={windowSize.width - 360}
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
