import React, { useRef, useLayoutEffect, useState } from "react";
import PropTypes from "prop-types";

import { format } from "d3-format";
import { timeFormat } from "d3-time-format";

import { ChartCanvas, Chart } from "react-stockcharts";
import { BarSeries, CandlestickSeries } from "react-stockcharts/lib/series";
import { XAxis, YAxis } from "react-stockcharts/lib/axes";
import {
    CrossHairCursor,
    MouseCoordinateX,
    MouseCoordinateY
} from "react-stockcharts/lib/coordinates";

import { discontinuousTimeScaleProvider } from "react-stockcharts/lib/scale";
import { OHLCTooltip } from "react-stockcharts/lib/tooltip";
import { fitWidth } from "react-stockcharts/lib/helper";
import { last } from "react-stockcharts/lib/utils";

const TradeChart = (props) => {
    const targetRef = useRef();
    const [dimensions, setDimensions] = useState({ width: 0, height: 0 });

    useLayoutEffect(() => {
        if (targetRef.current) {
            setDimensions({
                width: targetRef.current.offsetWidth,
                height: targetRef.current.offsetHeight
            });
            console.log(dimensions);
        }
    }, []);

    const type = props.type;
    const initialData = props.initialData;
    const width = props.width;
    const ratio = props.ratio;

    initialData.forEach((e) => {
        e.date = new Date(e.date);
    })

    const xScaleProvider = discontinuousTimeScaleProvider.inputDateAccessor(
        d => d.date
    );

    const { data, xScale, xAccessor, displayXAccessor } = xScaleProvider(
        initialData
    );

    const start = xAccessor(last(data));
    const end = xAccessor(data[Math.max(0, data.length - 150)]);
    const xExtents = [start, end];

    if (dimensions.width == 0) {
        return (
            <div ref={targetRef}>
                <p>Loading...</p>
                <p></p>
            </div>
        )
    } else {
        return (
            <ChartCanvas ref={targetRef}
                height={600}
                ratio={1}
                width={dimensions.width}
                margin={{ left: 100, right: 100, top: 100, bottom: 30 }}
                type={type}
                seriesName="BTC_EUR"
                data={data}
                xScale={xScale}
                xAccessor={xAccessor}
                displayXAccessor={displayXAccessor}
                xExtents={xExtents}
            >
                <Chart id={1} yExtents={[d => [d.high, d.low]]}>
                    <XAxis axisAt="bottom" orient="bottom" />
                    <YAxis axisAt="right" orient="right" ticks={5} />
                    <MouseCoordinateY
                        at="right"
                        orient="right"
                        displayFormat={format(".2f")}
                    />
                    <CandlestickSeries />
                    <OHLCTooltip forChart={1} origin={[-40, 0]} />
                </Chart>
                <Chart
                    id={2}
                    height={150}
                    yExtents={d => d.volume}
                    origin={(w, h) => [0, h - 150]}
                >
                    <YAxis
                        axisAt="left"
                        orient="left"
                        ticks={5}
                        tickFormat={format(".2s")}
                    />

                    <MouseCoordinateX
                        at="bottom"
                        orient="bottom"
                        displayFormat={timeFormat("%Y-%m-%d %H:%M")}
                    />
                    <MouseCoordinateY
                        at="left"
                        orient="left"
                        displayFormat={format(".4s")}
                    />

                    <BarSeries
                        yAccessor={d => d.volume}
                        fill={d => (d.close > d.open ? "#6BA583" : "#FF0000")}
                    />
                </Chart>
                <CrossHairCursor />
            </ChartCanvas>
        );
    }
};

export default TradeChart;
