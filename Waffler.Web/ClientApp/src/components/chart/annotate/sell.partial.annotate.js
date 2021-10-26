import React from "react";
import { Annotate, SvgPathAnnotation } from "react-financial-charts";
import brokenHeartIcon from '../../../assets/paths/broken-heart';

const SellCompleteAnnotate = () => {
    const whenSell = (data) => {
        return data.tradeOrder !== undefined &&
            data.tradeOrder.tradeTypeId === 2 &&
            data.tradeOrder.amount === data.tradeOrder.filledAmount;;
    }

    const brokenHeart = {
        fill: 'rgba(220, 20, 60, 0.5)',
        path: () => brokenHeartIcon,
        pathWidth: 9,
        pathHeight: 9,
        y: ({ yScale, datum }) => yScale(datum.high + 100),
    };

    return (
        <Annotate with={SvgPathAnnotation} usingProps={brokenHeart} when={whenSell} />
    );
}
export default SellCompleteAnnotate;