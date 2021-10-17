import React from "react";
import ApexCharts from 'apexcharts'
import { series, options } from './SampleData'

const TradeChart = () => (
    <ApexCharts options={options} series={series} type="candlestick" height={350} />
);

export default TradeChart;
