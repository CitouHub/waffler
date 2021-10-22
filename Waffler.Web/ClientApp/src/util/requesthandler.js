import axios from "axios"
import Config from "./config"

export default {
    send: async (request) => {
        try {
            let url = Config.apiURL() + request.url;
            if (request.method == 'GET') {
                return await axios.get(url);
            } else if (request.method == 'POST') {
                return await axios.post(url, request.data);
            }
        } catch (error) {
            console.error(error);
        }
    }
}