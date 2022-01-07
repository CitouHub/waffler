import Request from "../util/requesthandler"

export default {
    newTradeRule: async () => await Request.send({
        url: `/traderule`,
        method: 'POST'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    copyTradeRule: async (tradeRuleId) => await Request.send({
        url: `/traderule/copy/${tradeRuleId}`,
        method: 'POST'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    importTradeRule: async (tradeRule) => await Request.send({
        url: `/traderule/import`,
        method: 'POST',
        data: tradeRule
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    getTradeRules: async () => await Request.send({
        url: `/traderule`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    getTradeRule: async (tradeRuleId) => await Request.send({
        url: `/traderule/${tradeRuleId}`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    getTradeRuleForExport: async (tradeRuleId) => await Request.send({
        url: `/traderule/export/${tradeRuleId}`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    getTradeRuleAttributes: async () => await Request.send({
        url: `/traderule/attribute`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    updateTradeRule: async (tradeRule) => await Request.send({
        url: `/traderule`,
        method: 'PUT',
        data: tradeRule
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    deleteTradeRule: async (tradeRuleId) => await Request.send({
        url: `/traderule/${tradeRuleId}`,
        method: 'DELETE'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    startTradeRuleTest: async (tradeRuleTest) => await Request.send({
        url: `/traderule/test/start`,
        method: 'POST',
        data: tradeRuleTest
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    getTradeRuleTestStatus: async (tradeRuleId) => await Request.send({
        url: `/traderule/test/status/${tradeRuleId}`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    abortTradeRuleTest: async (tradeRuleId) => await Request.send({
        url: `/traderule/test/abort/${tradeRuleId}`,
        method: 'POST'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
}