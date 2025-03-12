import "./App.css";

import { Router } from "../router/Router";
import LoadingScreen from "../components/LoadingScreen/LoadingScreen";
import { useWristbandAuth } from "../providers/auth";

function App() {
  /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
  const { isAuthenticated } = useWristbandAuth();

  return (
    isAuthenticated ? <Router /> : <LoadingScreen />
  );
}

export default App;
