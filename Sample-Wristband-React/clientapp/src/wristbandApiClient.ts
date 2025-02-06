import axios from "axios";

import { redirectToLogin } from "./wristbandUtils";

/* CSRF_TOUCHPOINT */
const wristbandApiClient = axios.create({
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    xsrfCookieName: "XSRF-TOKEN",
    xsrfHeaderName: "X-XSRF-TOKEN",
    withXSRFToken: true,
});

/* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
// Any HTTP 401s should trigger the user to go log in again.  This happens when their
// session cookie has expired and/or the CSRF cookie/header are missing in the request.
// You can optionally catch HTTP 403s as well.
const unauthorizedAccessInterceptor = (error: { response: { status: number } }) => {
    if (error.response && [401, 403].includes(error.response.status)) {
        redirectToLogin("");
    }

    return Promise.reject(error);
};

wristbandApiClient.interceptors.response.use(undefined, unauthorizedAccessInterceptor);

export { wristbandApiClient };
