import React from 'react';
import { Annotate, SvgPathAnnotation } from 'react-financial-charts';
import btcIcon from '../../../assets/paths/btc';

const BuyNewAnnotate = () => {
    const whenBuy = (data) => {
        return data.tradeOrder !== undefined &&
            data.tradeOrder.tradeActionId === 1 &&
            data.tradeOrder.filledAmount !== 0 &&
            data.tradeOrder.filledAmount !== data.tradeOrder.amount &&
            data.tradeOrder.tradeOrderStatusId !== 10; //Test trade order
    }

    const btc = {
        fill: 'rgba(242, 169, 0, 1)',
        path: () => btcIcon,
        pathWidth: 9,
        pathHeight: 9,
        y: ({ yScale, datum }) => yScale(datum.high + 100),
    };

    return (
        <Annotate with={SvgPathAnnotation} usingProps={btc} when={whenBuy} />
    );
}
export default BuyNewAnnotate;