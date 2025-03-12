/* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
import { createContext, PropsWithChildren, useCallback, useEffect, useState } from "react";
import axios from "axios";

import { isUnauthorizedError, redirectToLogin, redirectToLogout } from "../../utils/wristband-utils";
import { AuthStatus, IWristbandAuthContext, SessionResponse } from "./types";

interface IWristbandAuthProviderProps<TSessionMetadata = unknown> {
  disableRedirectOnUnauthenticated?: boolean;
  loginUrl: string;
  logoutUrl: string;
  sessionUrl: string;
  xsrfCookieName?: string;
  xsrfHeaderName?: string;
  transformSessionMetadata?: (rawSessionMetadata: unknown) => TSessionMetadata;
}

const authProviderClient = axios.create({ withXSRFToken: true, withCredentials: true });

// React context responsbile for establishing that the user is authenticated and getting session data.
export const WristbandAuthContext = createContext<IWristbandAuthContext | undefined>(undefined);

// "AuthProvider" should wrap your App component to enable access to the "useAuth" hook everywhere.
// That hook can then be used to protect App routes.
export function WristbandAuthProvider<TSessionMetaData = unknown>({
  children,
  disableRedirectOnUnauthenticated = false,
  loginUrl,
  logoutUrl,
  sessionUrl,
  transformSessionMetadata,
  xsrfCookieName = 'XSRF-TOKEN',
  xsrfHeaderName = 'X-XSRF-TOKEN'
}: PropsWithChildren<IWristbandAuthProviderProps<TSessionMetaData>>) {
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [userId, setUserId] = useState<string>('');
  const [tenantId, setTenantId] = useState<string>('');
  const [metadata, setMetadata] = useState<TSessionMetaData>({} as TSessionMetaData);

  const authStatus: AuthStatus = isLoading
    ? AuthStatus.LOADING
    : (isAuthenticated ? AuthStatus.AUTHENTICATED : AuthStatus.UNAUTHENTICATED);

  const updateMetadata = useCallback((newMetadata: Partial<TSessionMetaData>) => {
    setMetadata(prevData => ({
      ...prevData,
      ...newMetadata
    }));
  }, []);

  // Bootstrap the application with the authenticated user's session data.
  useEffect(() => {
    const fetchSession = async () => {
      try {
        // The session API will let React know if the user has a previously authenticated session.
        // If so, it will initialize session data.
        const response = await authProviderClient.get<SessionResponse>(sessionUrl, {
          headers: { "Content-Type": "application/json", Accept: "application/json" },
          xsrfCookieName,
          xsrfHeaderName,
        });
        const { isAuthenticated: respIsAuthenticated, userId, tenantId, metadata: rawMetadata } = response.data;

        if (!respIsAuthenticated) {
          // Don't call logout here to preserve the current page for when the user returns after re-authentication.
          if (disableRedirectOnUnauthenticated) {
            setIsAuthenticated(false);
            setIsLoading(false);
          } else {
            await redirectToLogin({ loginUrl });
          }
        } else {
          setIsAuthenticated(true);
          setIsLoading(false);
          setUserId(userId);
          setTenantId(tenantId);

          // Apply transformation if provided
          if (rawMetadata) {
            setMetadata(
              transformSessionMetadata
                ? transformSessionMetadata(rawMetadata)
                : (rawMetadata as TSessionMetaData)
            );
          }
        }
      } catch (error: unknown) {
        console.log(error);
        if (disableRedirectOnUnauthenticated) {
          setIsAuthenticated(false);
          setIsLoading(false);
        } else {
          // Don't call logout on 401 to preserve the current page for when the user returns after re-authentication.
          isUnauthorizedError(error) ? await redirectToLogin({ loginUrl }) : await redirectToLogout({ logoutUrl });
        }
      }
    };

    fetchSession();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <WristbandAuthContext.Provider value={{
      authStatus,
      isAuthenticated,
      isLoading,
      metadata,
      tenantId,
      updateMetadata,
      userId
    }}>
      {children}
    </WristbandAuthContext.Provider>
  );
}
