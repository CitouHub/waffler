﻿import { format } from "d3-format";
import { timeFormat } from "d3-time-format";

export default {
    getToolTip: (currentItem, xAccessor) => {
        const dateFormat = timeFormat("%Y-%m-%d %H:%M");
        const numberFormat = format(".2f");

        let toolTip = {
            x: dateFormat(xAccessor(currentItem)),
            y: [
                {
                    label: "high",
                    value: currentItem.high && numberFormat(currentItem.high),
                },
                {
                    label: "open",
                    value: currentItem.open && numberFormat(currentItem.open),
                },
                {
                    label: "close",
                    value: currentItem.close && numberFormat(currentItem.close),
                },
                {
                    label: "low",
                    value: currentItem.low && numberFormat(currentItem.low),
                },
                {
                    label: "volume",
                    value: currentItem.low && numberFormat(currentItem.volume),
                },
            ]
        }

        if (currentItem.tradeOrder) {
            toolTip.y.push(
                {
                    label: "",
                    value: ""
                },
                {
                    label: "--------------------",
                    value: "--------------------"
                },
                {
                    label: currentItem.tradeOrder.tradeActionName,
                    value: currentItem.tradeOrder.tradeRuleName
                },
                {
                    label: "date",
                    value: dateFormat(currentItem.tradeOrder.orderDateTime)
                },
                {
                    label: "price",
                    value: currentItem.tradeOrder.price
                },
                {
                    label: "amount",
                    value: currentItem.tradeOrder.amount
                },
                {
                    label: "filled",
                    value: currentItem.tradeOrder.filledAmount
                },
                {
                    label: "status",
                    value: currentItem.tradeOrder.tradeOrderStatusName
                }
            );
        }

        return toolTip;
    }
}