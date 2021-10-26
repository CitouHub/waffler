import { format } from "d3-format";
import { timeFormat } from "d3-time-format";

export default {
    getToolTip: (currentItem, xAccessor) => {
        const dateFormat = timeFormat("%Y-%m-%d");
        const numberFormat = format(".2f");

        let toolTip = {
            x: dateFormat(xAccessor(currentItem)),
            y: [
                {
                    label: "open",
                    value: currentItem.open && numberFormat(currentItem.open),
                },
                {
                    label: "high",
                    value: currentItem.high && numberFormat(currentItem.high),
                },
                {
                    label: "low",
                    value: currentItem.low && numberFormat(currentItem.low),
                },
                {
                    label: "close",
                    value: currentItem.close && numberFormat(currentItem.close),
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
                    label: "type",
                    value: currentItem.tradeOrder.tradeTypeId
                },
                {
                    label: "rule",
                    value: currentItem.tradeOrder.tradeRuleId
                },
                {
                    label: "date",
                    value: currentItem.tradeOrder.orderDateTime.toISOString().substring(0, 16)
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
                }
            );
        }

        return toolTip;
    }
}