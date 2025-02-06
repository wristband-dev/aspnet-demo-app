import { useCallback, useState } from "react";
import axios from "axios";

import csharpLogo from "../assets/csharp.png";
import reactLogo from "../assets/react.svg";
import wristbandLogo from "../assets/wristband.png";
import { wristbandApiClient } from "../wristbandApiClient";
import { redirectToLogout } from "../wristbandUtils";

const HomePage = () => {
    const [count, setCount] = useState(0);
    const [protectedResult, setProtectedResult] = useState(0);
    const [unprotectedResult, setUnprotectedResult] = useState(0);

    const callProtectedEndpoint = useCallback(async () => {
        try {
            const response = await wristbandApiClient.get("/api/protected");
            const result = response.data as { message: string, value: number };
            setProtectedResult((prior) => prior + result.value);
        } catch (error) {
            if (axios.isAxiosError(error)) {
                console.error('Axios Error:', error.response?.status, error.response?.data);
            } else {
                console.error('Unexpected Error:', error);
            }
        }
    }, [setProtectedResult]);

    const callUnprotectedEndpoint = useCallback(async () => {
      try {
          const response = await wristbandApiClient.get("/api/unprotected");
          const result = response.data as { message: string, value: number };
          setUnprotectedResult((prior) => prior + result.value);
      } catch (error) {
          if (axios.isAxiosError(error)) {
              console.error('Axios Error:', error.response?.status, error.response?.data);
          } else {
              console.error('Unexpected Error:', error);
          }
      }
  }, [setUnprotectedResult]);

    return (
        <>
            <div>
                <a href="https://dotnet.microsoft.com/en-us/languages/csharp" target="_blank">
                    <img src={csharpLogo} className="logo" alt="Csharp logo" />
                </a>
                <a href="https://react.dev" target="_blank">
                    <img src={reactLogo} className="logo" alt="React logo" />
                </a>
                <a href="https://wristband.dev" target="_blank">
                    <img src={wristbandLogo} className="logo react" alt="Wristband logo" />
                </a>
            </div>
            <h1>C# + React + Wristband</h1>
            <div className="card">
                <button onClick={() => setCount((value) => value + 1)}>
                    Local count {count}
                </button>
            </div>
            <div className="card">
                <button onClick={callProtectedEndpoint}>
                    Protected API count {protectedResult}
                </button>
            </div>
            <div className="card">
                <button onClick={callUnprotectedEndpoint}>
                    Unprotected API count {unprotectedResult}
                </button>
            </div>
            <div className="card">
                <button onClick={redirectToLogout}>
                    Logout
                </button>
            </div>
        </>
    );
};

export { HomePage };
