import Request from "../util/requesthandler"

export default {
    getCandleStickss: async (from, to, tradeTypeId, periodMinutes) => await Request.send({
        url: `/candlestick/
            ${from.toISOString().split('T')[0]}/
            ${to.toISOString().split('T')[0]}/
            ${tradeTypeId}/
            ${periodMinutes}`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
}