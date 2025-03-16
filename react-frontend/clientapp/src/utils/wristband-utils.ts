import { AxiosError } from 'axios';

// ////////////////////////////////////
//   LOGIN UTILS
// ////////////////////////////////////

/**
 * Configuration options for redirecting to your server's login endpoint.
 * @interface LoginRedirectConfig
 */
interface LoginRedirectConfig {
  /**
   * Optional hint to pre-fill the Tenant Login Page form with a specific username or email.
   * @type {string}
   */
  loginHint?: string;

  /**
   * The login URL to redirect to. If not provided, defaults to `/api/auth/login` at the current origin.
   * @type {string}
   */
  loginUrl?: string

  /**
   * URL to redirect back to after successful authentication.
   * If not provided and omitReturnUrl is false, uses the current page URL.
   * @type {string}
   */
  returnUrl?: string;

  /**
   * When set to true, no return_url parameter will be included in the login redirect URL.
   * @type {boolean}
   */
  omitReturnUrl?: boolean;
}

/**
 * Redirects the user to your server's login endpoint with optional configuration.
 *
 * @param {LoginRedirectConfig} config - Configuration options for the login redirect
 * @returns {Promise<void>} A promise that resolves when the redirect is triggered
 *
 * @example
 * // Basic redirect to login endpoint:
 * await redirectToLogin();
 *
 * @example
 * // Redirect with login hint:
 * await redirectToLogin({ loginHint: 'user@example.com' });
 *
 * @example
 * // Redirect to specific login URL along with a return URL:
 * await redirectToLogin({
 *   loginUrl: 'https://auth.example.com/login',
 *   returnUrl: 'https://app.example.com/dashboard'
 * });
 */
export async function redirectToLogin(config: LoginRedirectConfig = {}) {
  const location = config.loginUrl ?? `${window.location.origin}/api/auth/login`;
  const searchParamsString = new URLSearchParams({
      ...(config.loginHint ? { login_hint: config.loginHint } : {}),
      ...(config.omitReturnUrl ? {} : { return_url: encodeURI(config.returnUrl ?? window.location.href) }),
    }).toString();
  const query = searchParamsString ? `?${searchParamsString}` : '';

  window.location.href = `${location}${query}`;
}

// ////////////////////////////////////
//   LOGOUT UTILS
// ////////////////////////////////////

/**
 * Configuration options for redirecting to your server's logout endpoint.
 * @interface LogoutRedirectConfig
 */
interface LogoutRedirectConfig {
  /**
   * Custom logout URL to redirect to. If not provided, defaults to `/api/auth/logout` at the current origin.
   * @type {string}
   */
  logoutUrl?: string;
}

/**
 * Redirects the user to your server's logout endpoint with optional configuration.
 *
 * @param {LogoutRedirectConfig} config - Configuration options for the logout redirect
 * @returns {Promise<void>} A promise that resolves when the redirect is triggered
 *
 * @example
 * // Basic redirect to default logout endpoint
 * await redirectToLogout();
 *
 * @example
 * // Redirect to custom logout URL
 * await redirectToLogout({
 *   logoutUrl: 'https://auth.example.com/logout'
 * });
 */
export async function redirectToLogout(config: LogoutRedirectConfig = {}) {
  const location = config.logoutUrl ?? `${window.location.origin}/api/auth/logout`;
  window.location.href = location;
}

// ////////////////////////////////////
//   HTTP ERROR UTILS
// ////////////////////////////////////

/**
 * Checks if an error represents a specific HTTP status code error.
 *
 * @param {unknown} error - The error to check. Must be either an AxiosError or a Response object.
 * @param {number} statusCode - The HTTP status code to check for.
 * @returns {boolean} True if the error has the specified status code, false otherwise.
 * @throws {TypeError} If the error is null, undefined, or not an AxiosError or Response object.
 *
 * @example
 * // With Axios
 * try {
 *   await axios.get('/api/resource');
 * } catch (error) {
 *   if (isHttpStatusError(error, 404)) {
 *     console.log('Resource not found');
 *   }
 * }
 *
 * @example
 * // With Fetch
 * const response = await fetch('/api/resource');
 * if (isHttpStatusError(response, 401)) {
 *   console.log('Authentication required');
 * }
 */
export function isHttpStatusError(error: unknown, statusCode: number): boolean {
  // Handle null/undefined case with an exception
  if (error === null || error === undefined) {
    throw new TypeError('Argument [error] cannot be null or undefined');
  }

  // Handle Axios error format
  if (error instanceof AxiosError) {
    return error.response?.status === statusCode;
  }

  // Handle fetch Response objects
  if (error instanceof Response) {
    return error.status === statusCode;
  }

  // If it's neither of the expected types, throw an error.
  throw new TypeError(
    `Invalid error type: Expected either an AxiosError or a Response object, but received type: [${typeof error}] `
  );
}

/**
 * Checks if an error represents an HTTP 401 Unauthorized error.
 *
 * @param {unknown} error - The error to check. Must be either an AxiosError or a Response object.
 * @returns {boolean} True if the error has a 401 status code, false otherwise.
 * @throws {TypeError} If the error is null, undefined, or not an AxiosError or Response object.
 *
 * @example
 * // With Axios
 * try {
 *   await axios.get('/api/resource');
 * } catch (error) {
 *   if (isUnauthorizedError(error)) {
 *     console.log('Authentication required');
 *   }
 * }
 *
 * @example
 * // With Fetch
 * const response = await fetch('/api/resource');
 * if (isUnauthorizedError(response)) {
 *   console.log('Authentication required');
 * }
 */
export const isUnauthorizedError = (error: unknown) => isHttpStatusError(error, 401);

/**
 * Checks if an error represents an HTTP 403 Forbidden error.
 *
 * @param {unknown} error - The error to check. Must be either an AxiosError or a Response object.
 * @returns {boolean} True if the error has a 403 status code, false otherwise.
 * @throws {TypeError} If the error is null, undefined, or not an AxiosError or Response object.
 *
 * @example
 * // With Axios
 * try {
 *   await axios.get('/api/resource');
 * } catch (error) {
 *   if (isForbiddenError(error)) {
 *     console.log('Forbidden');
 *   }
 * }
 *
 * @example
 * // With Fetch
 * const response = await fetch('/api/resource');
 * if (isForbiddenError(response)) {
 *   console.log('Forbidden');
 * }
 */
export const isForbiddenError = (error: unknown) => isHttpStatusError(error, 403);

// ////////////////////////////////////
//   RBAC UTILS
// ////////////////////////////////////

export function isOwnerRole(roleName: string) {
  // Should match the Role "name" field, i.e. "app:invotasticb2b:owner"
  return /^app:.*:owner$/.test(roleName);
}
