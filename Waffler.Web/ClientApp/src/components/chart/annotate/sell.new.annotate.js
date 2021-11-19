import React from 'react';
import { Annotate, SvgPathAnnotation } from 'react-financial-charts';
import brokenHeartIcon from '../../../assets/paths/broken-heart';

const SellCompleteAnnotate = () => {
    const whenSell = (data) => {
        return data.tradeOrder !== undefined &&
            data.tradeOrder.tradeActionId === 2 &&
            data.tradeOrder.amount === data.tradeOrder.filledAmount &&
            data.tradeOrder.isTestOrder === false;
    }

    const brokenHeart = {
        fill: '#909090',
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