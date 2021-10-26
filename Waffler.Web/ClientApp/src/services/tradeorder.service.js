﻿import Request from "../util/requesthandler"

export default {
    getTradeOrders: async (from, to) => await Request.send({
        url: `/tradeorder?from=${from.toISOString().split('T')[0]}&to=${to.toISOString().split('T')[0]}`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
}