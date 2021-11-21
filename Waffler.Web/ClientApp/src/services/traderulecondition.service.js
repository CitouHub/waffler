import Request from "../util/requesthandler"

export default {
    newTradeRuleCondition: async (tradeRuleId) => await Request.send({
        url: `/traderulecondition/${tradeRuleId}`,
        method: 'POST'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    getTradeRuleConditions: async (tradeRuleId) => await Request.send({
        url: `/traderulecondition/${tradeRuleId}`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    getTradeRuleConditionAttributes: async () => await Request.send({
        url: `/traderulecondition/attribute`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    updateTradeRuleCondition: async (tradeRuleCondition) => await Request.send({
        url: `/traderuleCondition`,
        method: 'PUT',
        data: tradeRuleCondition
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    deleteTradeRuleCondition: async (tradeRuleConditionId) => await Request.send({
        url: `/traderuleCondition/${tradeRuleConditionId}`,
        method: 'DELETE'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
}