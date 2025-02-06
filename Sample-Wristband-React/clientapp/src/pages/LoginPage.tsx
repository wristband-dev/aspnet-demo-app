import { redirectToLogin } from "../wristbandUtils.ts";
import { useEffect } from "react";

const LoginPage = () => {
    useEffect(() => {
        const redirect = async () => {
            await redirectToLogin("");
        };

        // noinspection JSIgnoredPromiseFromCall
        redirect();
    }, []);

    return null;
};

export { LoginPage };
