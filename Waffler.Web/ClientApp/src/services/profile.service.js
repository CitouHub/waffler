import Request from "../util/requesthandler"

export default {
    hasProfile: async () => await Request.send({
        url: `/profile/exists`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    createProfile: async (password) => await Request.send({
        url: `/profile`,
        data: { password: password },
        method: 'POST'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    verifyPassword: async (password) => await Request.send({
        url: `/profile/password/verify`,
        data: { password: password },
        method: 'POST'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    getBalance: async () => await Request.send({
        url: `/profile/bitpanda/balance`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
}