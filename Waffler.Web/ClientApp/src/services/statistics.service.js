﻿import Request from "../util/requesthandler"

export default {
    getTradeRuleBuyStatistics: async (from, to, mode) => await Request.send({
        url: `/statistics/buy/
            ${from.toISOString().split('T')[0]}/
            ${to.toISOString().split('T')[0]}/
            ${mode}`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
}