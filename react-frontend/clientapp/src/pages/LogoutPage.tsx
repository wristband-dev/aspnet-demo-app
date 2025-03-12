import { useEffect } from "react";

import { redirectToLogout } from "../utils/wristband-utils.ts";

const LogoutPage = () => {
  useEffect(() => {
    const redirect = async () => {
      await redirectToLogout();
    };

    redirect();
  }, []);

  return <div />;
};

export { LogoutPage };
