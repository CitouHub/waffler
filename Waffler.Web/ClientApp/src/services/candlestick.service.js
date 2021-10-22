import Request from "../util/requesthandler"

export default {
    getCandleStickss: async (from, to, tradeTypeId, periodMinutes) => await Request.send({
        url: `/tradeorder?from=${from.toISOString().split('T')[0]}&to=${to.toISOString().split('T')[0]}`,
        method: 'GET'
    })
}