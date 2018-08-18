var retries = 0;
axios.interceptors.response.use(
    response => response,
    (error) => {
        if (retries < 3) {
            const originalRequest = error.config
            retries++;
            return axios(originalRequest)
        }
        else{
            retries = 0;
            return Promise.reject(error)
        }
    }
)