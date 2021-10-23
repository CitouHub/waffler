import React, { useRef, useLayoutEffect, useState, useEffect } from "react";
import {
    Annotate,
    ema,
    discontinuousTimeScaleProviderBuilder,
    CandlestickSeries,
    Chart,
    ChartCanvas,
    LabelAnnotation,
    SvgPathAnnotation,
    XAxis,
    YAxis,
    withDeviceRatio,
    withSize,
} from "react-financial-charts";

import LoadingBar from '../utils/loadingbar'
import CandleStickService from '../../services/candlestick.service'

import './chart.css';

const TradeChart = (props) => {
    const canvasRef = useRef();
    const [dimensions, setDimensions] = useState({ width: 0, height: 0 });
    const [loading, setLoading] = useState(true);
    const [initialData, setInitialData] = useState([]);

    useLayoutEffect(() => {
        if (canvasRef.current) {
            setDimensions({
                width: canvasRef.current.offsetWidth,
                height: canvasRef.current.offsetHeight
            });
        }
        CandleStickService.getCandleStickss(new Date('2021-10-01'), new Date('2021-10-30'), 1, 15).then((result) => {
            result.forEach((e) => {
                e.date = new Date(e.date);
            })
            setInitialData(result);
            setLoading(false);
        })
    }, []);

    if (loading) {
        return (
            <div ref={canvasRef}>
                <LoadingBar active={loading} />
            </div>
        )
    } else if (dimensions.width != 0 && initialData.length > 0) {
        const labelAnnotation = {
            fill: "#2196f3",
            text: "Monday",
            pageYOffset: 50,
            offsetHeight: 50,
            y: ({ yScale, datum }) => yScale(datum.high),
        };
        
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

        const calculatedData = ema12(initialData);

        const { data, xScale, xAccessor, displayXAccessor } = xScaleProvider(calculatedData);

        const max = xAccessor(data[data.length - 1]);
        const min = xAccessor(data[Math.max(0, data.length - 100)]);
        const xExtents = [min, max];

        const when = (data) => {
            return data.date.getDay() === 1;
        };

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
                seriesName="Data"
                xScale={xScale}
                xAccessor={xAccessor}
                xExtents={xExtents}
            >
                <Chart id={1} yExtents={yExtents}>
                    <XAxis showGridLines />
                    <YAxis showGridLines />
                    <CandlestickSeries />
                    <Annotate with={LabelAnnotation} usingProps={labelAnnotation} when={when} />
                </Chart>
            </ChartCanvas>
        );
    } else {
        return null;
    }
};

export default TradeChart;
