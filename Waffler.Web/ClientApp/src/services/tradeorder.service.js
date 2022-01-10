import Request from "../util/requesthandler"

export default {
    getTradeOrders: async (from, to) => await Request.send({
        url: `/tradeorder?from=${from.toISOString().split('T')[0]}&to=${to.toISOString().split('T')[0]}`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    getTradeOrderStatuses: async () => await Request.send({
        url: `/tradeorder/status`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    anyTradeOrders: async (tradeRuleId) => await Request.send({
        url: `/tradeorder/any/${tradeRuleId}`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    setTradeRule: async (tradeOrderId, tradeRuleId) => await Request.send({
        url: `/tradeorder/${tradeOrderId}/traderule/${tradeRuleId}`,
        method: 'PUT'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    deleteTestTradeOrders: async (tradeRuleId) => await Request.send({
        url: `/tradeorder/test/${tradeRuleId}`,
        method: 'DELETE'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
}