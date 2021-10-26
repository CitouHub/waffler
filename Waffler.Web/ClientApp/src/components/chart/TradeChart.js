import React, { useRef, useState, useEffect } from "react";
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

import LoadingBar from '../utils/loadingbar'
import BuyNewAnnotate from './annotate/buy.new.annotate'
import BuyPartialAnnotate from './annotate/buy.partial.annotate'
import BuyCompleteAnnotate from './annotate/buy.complete.annotate'
import SellNewAnnotate from './annotate/sell.new.annotate'
import SellPartialAnnotate from './annotate/sell.partial.annotate'
import SellCompleteAnnotate from './annotate/sell.complete.annotate'
import CandleStickService from '../../services/candlestick.service'
import TradeOrderService from '../../services/tradeorder.service'
import ToolTipHelper from './tooltip/hover.tooltip'

import './chart.css';

const TradeChart = (props) => {
    const canvasRef = useRef();
    const [dimensions, setDimensions] = useState({ width: 0, height: 0 });
    const [loading, setLoading] = useState(true);
    const [candleSticks, setCandleSticks] = useState([]);
    const [tradeOrders, setTradeOrder] = useState([]);

    useEffect(() => {
        if (canvasRef.current) {
            setDimensions({
                width: canvasRef.current.offsetWidth,
                height: canvasRef.current.offsetHeight
            });
        }
        CandleStickService.getCandleStickss(new Date('2021-10-01'), new Date('2021-10-30'), 1, 15).then((candleSticksResult) => {
            candleSticksResult.forEach((e) => {
                e.date = new Date(e.date);
            });

            TradeOrderService.getTradeOrders(new Date('2021-10-01'), new Date('2021-10-30')).then((tradeOrdersResult) => {
                console.log(tradeOrdersResult);
                tradeOrdersResult.forEach((tradeOrder) => {
                    tradeOrder.orderDateTime = new Date(tradeOrder.orderDateTime);
                    let candleStick = candleSticksResult.find((candleStick) => {
                        if (candleStick.date >= tradeOrder.orderDateTime) {
                            return candleStick;
                        }
                    });

                    candleStick.tradeOrder = tradeOrder;
                });
                console.log(candleSticksResult);
                setCandleSticks(candleSticksResult);
                setLoading(false);
            });
        })
    }, []);

    if (loading) {
        return (
            <div ref={canvasRef}>
                <LoadingBar active={loading} />
            </div>
        )
    } else if (dimensions.width != 0 && candleSticks.length > 0) {
        const margin = { left: 0, right: 48, top: 100, bottom: 24 };
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

        const calculatedData = ema12(candleSticks);

        const { data, xScale, xAccessor, displayXAccessor } = xScaleProvider(calculatedData);

        const max = xAccessor(data[data.length - 1]);
        const min = xAccessor(data[Math.max(0, data.length - 100)]);
        const xExtents = [min, max];
        const yExtents = (data) => {
            return [data.high, data.low];
        };

        return (
            <ChartCanvas
                height={600}
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
                    <SellNewAnnotate />
                    <SellPartialAnnotate />
                    <SellCompleteAnnotate />
                    <HoverTooltip
                        yAccessor={ema12.accessor()}
                        tooltip={{content: ({ currentItem, xAccessor }) => ToolTipHelper.getToolTip(currentItem, xAccessor)}}
                    />
                </Chart>
            </ChartCanvas>
        );
    } else {
        return null;
    }
};

export default TradeChart;
