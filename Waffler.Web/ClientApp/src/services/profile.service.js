import Request from "../util/requesthandler"

export default {
    hasProfile: async () => await Request.send({
        url: `/profile/exists`,
        method: 'GET'
    }).then((response) => {
        return response.data;
    }),
    createProfile: async (password) => await Request.send({
        url: `/profile`,
        data: { password: password },
        method: 'POST'
    })
}