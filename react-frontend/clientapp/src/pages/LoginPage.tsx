import { useEffect } from "react";

import { redirectToLogin } from "@wristband/react-client-auth";

const LoginPage = () => {
  useEffect(() => {
    const redirect = async () => {
      await redirectToLogin('/api/auth/login');
    };

    redirect();
  }, []);

    return null;
};

export { LoginPage };
