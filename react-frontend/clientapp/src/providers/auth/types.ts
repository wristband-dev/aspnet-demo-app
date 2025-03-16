export enum AuthStatus {
  LOADING = 'loading',
  AUTHENTICATED = 'authenticated',
  UNAUTHENTICATED = 'unauthenticated'
}


export interface IWristbandAuthContext<TSessionMetadata = unknown> {
  authStatus: AuthStatus;
  isAuthenticated: boolean;
  isLoading: boolean;
  metadata: TSessionMetadata;
  userId: string;
  tenantId: string;
  updateMetadata: (newMetadata: Partial<TSessionMetadata>) => void;
}

export interface SessionResponse {
  isAuthenticated: boolean;
  metadata: unknown;
  userId: string;
  tenantId: string;
}
