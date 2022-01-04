import React, { useState, useEffect } from "react";
import StatisticsFilter from '../../../filter/statistics.filter';
import LoadingBar from '../../../utils/loadingbar';

import CandleStickService from '../../../../services/candlestick.service';
import StatisticsService from '../../../../services/statistics.service';

import { ReactComponent as BTC } from '../../../../assets/images/svg/bitcoin.svg';
import { ReactComponent as TrendUp } from '../../../../assets/images/svg/trend-up.svg';
import { ReactComponent as TrendDown } from '../../../../assets/images/svg/trend-down.svg';

import '../../../statistics/statistics.css';

const TradeRuleBuyStatistics = () => {
    let toDate = new Date();

    const [loading, setLoading] = useState(true);
    const [loadingPeriod, setloadingPeriod] = useState(true);
    const [statistics, setStatistics] = useState([]);
    const [trend, setTrend] = useState([]);
    const [filter, setFilter] = useState({
        fromDate: toDate,
        toDate: toDate,
        statisticsMode: 1
    });

    useEffect(() => {
        setloadingPeriod(true);
        CandleStickService.getFirstPeriod().then(result => {
            var date = new Date(result);
            date.setDate(date.getDate() + 1);
            setFilter({ ...filter, fromDate: date });

            setloadingPeriod(false);
        });
    }, []);

    useEffect(() => {
        if (loadingPeriod === false) {
            setLoading(true);
            var getTradeRuleBuyStatistics = StatisticsService.getTradeRuleBuyStatistics(filter.fromDate, filter.toDate, filter.statisticsMode);
            var getTrend = StatisticsService.getTrend(filter.fromDate, filter.toDate, 1, 240);

            Promise.all([getTradeRuleBuyStatistics, getTrend]).then((result) => {
                setStatistics(result[0]);
                setTrend(result[1]);
                setLoading(false);
            });
        }
    }, [loadingPeriod, filter]);

    return (
        <div>
            <LoadingBar active={loading} />
            <div className='mt-3 mb-3'>
                <h4>Statistics</h4>
            </div>
            {loadingPeriod === false && <div className="stat-header">
                <div>
                    <StatisticsFilter filter={filter} updateFilter={(filter) => setFilter(filter)} />
                </div>
                {loading === false && trend && <div className="d-flex">
                    {<BTC className="stat-icon btc-icon" />}
                    {trend.change >= 0 && <TrendUp className="stat-icon trend-up" />}
                    {trend.change <= 0 && <TrendDown className="stat-icon trend-down" />}
                    <span className={"trend-change " + (trend.change < 0 ? 'trend-down' : 'trend-up')}>{trend.change} %</span>
                </div>}
            </div>}
            <div className='mt-4'>
                <table className='stat-table'>
                    <thead>
                        <tr>
                            <th></th>
                            <th>Amount</th>
                            <th>Filled</th>
                            <th>Invested</th>
                            <th>Avg. price</th>
                            <th>Change</th>
                        </tr>
                    </thead>
                    <tbody>
                        {statistics && statistics.length > 0 && statistics.map(function (stat) {
                            return (
                                <tr key={stat.tradeRuleId}>
                                    <th>{stat.tradeRuleName}</th>
                                    <td>{stat.totalAmount} ₿</td>
                                    <td className={stat.filledPercent < 75 ? 'trend-down' : 'trend-up'}>{stat.filledPercent} %</td>
                                    <td>{stat.totalInvested} €</td>
                                    <td>{stat.averagePrice} €</td>
                                    <td className={stat.valueIncrease < 0 ? 'trend-down' : 'trend-up'}>{stat.valueIncrease} %</td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>
            </div>
        </div>
    )
};

export default TradeRuleBuyStatistics;