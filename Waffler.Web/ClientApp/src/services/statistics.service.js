import Request from "../util/requesthandler"

export default {
    getTradeRuleBuyStatistics: async (from, to, tradeTypeId, tradeSelection) => await Request.send({
        url: `/statistics/buy/
            ${from.toISOString().split('T')[0]}/
            ${to.toISOString().split('T')[0]}/
            ${tradeTypeId}`,
        data: tradeSelection,
        method: 'POST'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    getTrend: async (from, to, tradeTypeId, samplePeriodMinues) => await Request.send({
        url: `/statistics/trend/
            ${from.toISOString().split('T')[0]}/
            ${to.toISOString().split('T')[0]}/
            ${tradeTypeId}/
            ${samplePeriodMinues}`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
}