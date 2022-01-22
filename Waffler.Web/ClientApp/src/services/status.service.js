import Request from "../util/requesthandler"

export default {
    awaitDatabaseOnline: async () => await Request.send({
        url: `/status/database/await/online`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
    isDatabaseOnline: async () => await Request.send({
        url: `/status/database/online`,
        method: 'GET'
    }).then((response) => {
        return Request.handleResponse(response)
    }),
}