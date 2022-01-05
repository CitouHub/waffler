import Request from "../util/requesthandler"

export default {
    hasProfile: async () => await Request.send({
        url: `/profile/exists`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    getProfile: async () => await Request.send({
        url: `/profile`,
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
    updateProfile: async (profile) => await Request.send({
        url: `/profile`,
        data: profile,
        method: 'PUT'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    updatePassword: async (password) => await Request.send({
        url: `/profile/password`,
        data: password,
        method: 'PUT'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    login: async (password) => await Request.send({
        url: `/profile/login`,
        data: { password: password },
        method: 'POST'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    getBalance: async () => await Request.send({
        url: `/profile/balance`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    getLatestRelease: async () => await Request.send({
        url: `/profile/release/latest`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
}