import axios from "axios"
import Config from "./config"

export default {
    send: async (request) => {
        try {
            let url = Config.apiURL() + request.url;
            if (request.method === 'GET') {
                return await axios.get(url);
            } else if (request.method === 'POST') {
                return await axios.post(url, request.data);
            } else if (request.method === 'PUT') {
                return await axios.put(url, request.data);
            } else if (request.method === 'DELETE') {
                return await axios.delete(url);
            }
        } catch (error) {
            console.error(error);
        }
    },
    handleResponse: async (response) => {
        try {
            if (response) {
                if (response.status === 200) {
                    return response.data;
                } else if (response.status !== 204) {
                    console.warn("Unexpected API result!");
                    console.warn(response);
                }
            } else {
                console.warn("Unexpected NULL response from API");
            }
        } catch (error) {
            console.error(error);
        }
    }
}