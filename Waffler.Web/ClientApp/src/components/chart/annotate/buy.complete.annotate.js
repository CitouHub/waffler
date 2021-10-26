import React from "react";
import { Annotate, SvgPathAnnotation } from "react-financial-charts";
import btcIcon from '../../../assets/paths/btc';

const BuyCompleteAnnotate = () => {
    const whenBuy = (data) => {
        return data.tradeOrder !== undefined &&
            data.tradeOrder.tradeTypeId === 1 &&
            data.tradeOrder.amount === data.tradeOrder.filledAmount;
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
export default BuyCompleteAnnotate;