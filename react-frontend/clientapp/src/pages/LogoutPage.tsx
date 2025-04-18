import { useEffect } from "react";
import { redirectToLogout } from "@wristband/react-client-auth";

const LogoutPage = () => {
  useEffect(() => {
    const redirect = async () => {
      await redirectToLogout('/api/auth/logout');
    };

    redirect();
  }, []);

  return <div />;
};

export { LogoutPage };
