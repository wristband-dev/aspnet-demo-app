import { wristbandApiClient } from "./wristbandApiClient";

export async function redirectToLogout() {
    window.location.href = `${window.location.origin}/api/auth/logout`
}

export async function redirectToLogin(loginHint: string = '') {
    const query = new URLSearchParams({
        return_url: encodeURI(window.location.href),
        login_hint: loginHint,
     }).toString();
    window.location.href = `${window.location.origin}/api/auth/login?${query}`;
}

export function isOwnerRole(roleName: string) {
    // Should match the Role "name" field, i.e. "app:invotasticb2b:owner"
    return /^app:.*:owner$/.test(roleName);
}

export const getAuthState = async function () {
    const response = await wristbandApiClient.get("/api/session");
    return response.data.isAuthenticated;
};
