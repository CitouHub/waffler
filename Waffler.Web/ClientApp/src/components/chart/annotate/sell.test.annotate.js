import React from "react";
import { Annotate, SvgPathAnnotation } from "react-financial-charts";
import brokenHeartIcon from '../../../assets/paths/broken-heart';

const SellTestAnnotate = () => {
    const whenSell = (data) => {
        return data.tradeOrder !== undefined &&
            data.tradeOrder.tradeActionId === 2 &&
            data.tradeOrder.tradeOrderStatusId === 10; //Test trade order
    }

    const brokenHeart = {
        fill: 'rgba(77, 171, 245, 1)',
        path: () => brokenHeartIcon,
        pathWidth: 9,
        pathHeight: 9,
        y: ({ yScale, datum }) => yScale(datum.high + 100),
    };

    return (
        <Annotate with={SvgPathAnnotation} usingProps={brokenHeart} when={whenSell} />
    );
}
export default SellTestAnnotate;