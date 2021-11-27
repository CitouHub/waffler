import Request from "../util/requesthandler"

export default {
    getCandleSticks: async (from, to, tradeTypeId, periodMinutes) => await Request.send({
        url: `/candlestick/
            ${from.toISOString().split('T')[0]}/
            ${to.toISOString().split('T')[0]}/
            ${tradeTypeId}/
            ${periodMinutes}`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    resetCandleSticksSync: async () => await Request.send({
        url: `/candlestick/sync/reset`,
        method: 'POST'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    getCandleSticksSyncStatus: async () => await Request.send({
        url: `/candlestick/sync/status`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
}