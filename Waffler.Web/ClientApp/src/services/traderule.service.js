import Request from "../util/requesthandler"

export default {
    getTradeRules: async () => await Request.send({
        url: `/traderule`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
}