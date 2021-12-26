import React, { useState, useEffect } from "react";
import StatisticsFilter from '../../filter/filter';
import LoadingBar from '../../../utils/loadingbar';

import CandleStickService from '../../../../services/candlestick.service';
import StatisticsService from '../../../../services/statistics.service';

import '../../../statistics/statistics.css';

const TradeRuleBuyStatistics = () => {
    let toDate = new Date();

    const [loading, setLoading] = useState(true);
    const [loadingPeriod, setloadingPeriod] = useState(true);
    const [statistics, setStatistics] = useState([]);
    const [filter, setFilter] = useState({
        fromDate: toDate,
        toDate: toDate,
        statisticsMode: 1
    });

    useEffect(() => {
        setloadingPeriod(true);
        CandleStickService.getFirstPeriod().then(result => {
            setFilter({ ...filter, fromDate: new Date(result) });

            setloadingPeriod(false);
        });
    }, []);

    useEffect(() => {
        if (loadingPeriod === false) {
            setLoading(true);
            StatisticsService.getTradeRuleBuyStatistics(filter.fromDate, filter.toDate, filter.statisticsMode).then(result => {
                setStatistics(result);

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
            {loadingPeriod === false && <StatisticsFilter filter={filter} updateFilter={(filter) => setFilter(filter)} />}
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
                                    <td className={stat.filledPercent < 100 ? 'trend-down': 'trend-up'}>{stat.filledPercent} %</td>
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