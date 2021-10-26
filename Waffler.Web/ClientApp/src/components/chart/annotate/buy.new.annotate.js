import React from 'react';
import { Annotate, SvgPathAnnotation } from 'react-financial-charts';
import btcIcon from '../../../assets/paths/btc';

const BuyNewAnnotate = () => {
    const whenBuy = (data) => {
        return data.tradeOrder !== undefined &&
            data.tradeOrder.tradeTypeId === 1 &&
            data.tradeOrder.filledAmount === 0;
    }

    const btc = {
        fill: '#909090',
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