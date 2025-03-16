import React from 'react';
import ReactDOM from 'react-dom/client';

import './index.css';

import App from './app/App.tsx';
import { WristbandAuthProvider } from './providers/auth';
import { MySessionData, ApiSessionData } from './types';
import { isOwnerRole } from './utils/wristband-utils.ts';

// First, cast the unknown metadata to your session endpoint's data type.
// Then, transform it to your expected session data type.
const transformSessionMetadata = (metadata: unknown): MySessionData => {
  const apiSessionData = metadata as ApiSessionData;
  return {
    email: apiSessionData.email,
    fullName: apiSessionData.fullName,
    tenantDomainName: apiSessionData.tenantDomainName,
    hasOwnerRole: apiSessionData.roles.some(role => isOwnerRole(role.name))
  };
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    {/* WRISTBAND_TOUCHPOINT - AUTHENTICATION */}
    <WristbandAuthProvider<MySessionData>
      transformSessionMetadata={transformSessionMetadata}
      loginUrl='/api/auth/login'
      logoutUrl='/api/auth/logout'
      sessionUrl='/api/session'
    >
      <App />
    </WristbandAuthProvider>
  </React.StrictMode>
);
