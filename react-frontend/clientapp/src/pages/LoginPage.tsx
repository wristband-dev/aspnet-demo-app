import { useEffect } from "react";

import { redirectToLogin } from "../utils/wristband-utils.ts";

const LoginPage = () => {
  useEffect(() => {
    const redirect = async () => {
      await redirectToLogin();
    };

    redirect();
  }, []);

    return null;
};

export { LoginPage };
