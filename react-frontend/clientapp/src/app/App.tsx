import { useWristbandAuth } from '@wristband/react-client-auth';

import "./App.css";

import { Router } from "../router/Router";
import { LoadingScreen } from "../components/LoadingScreen";

function App() {
  /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
  const { isAuthenticated } = useWristbandAuth();

  return (
    isAuthenticated ? <Router /> : <LoadingScreen />
  );
}

export default App;
