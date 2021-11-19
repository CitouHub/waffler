import React from 'react';
import { Annotate, SvgPathAnnotation } from 'react-financial-charts';
import btcIcon from '../../../assets/paths/btc';

const BuyTestAnnotate = () => {
    const whenBuy = (data) => {
        return data.tradeOrder !== undefined &&
            data.tradeOrder.tradeActionId === 1 &&
            data.tradeOrder.isTestOrder === true;
    }

    const btc = {
        fill: 'rgba(77, 171, 245, 1)',
        path: () => btcIcon,
        pathWidth: 9,
        pathHeight: 9,
        y: ({ yScale, datum }) => yScale(datum.high + 100),
    };

    return (
        <Annotate with={SvgPathAnnotation} usingProps={btc} when={whenBuy} />
    );
}
export default BuyTestAnnotate;