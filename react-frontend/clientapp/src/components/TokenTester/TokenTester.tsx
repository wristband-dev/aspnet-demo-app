import { useState } from 'react';
import { useWristbandToken, redirectToLogin, WristbandError } from '@wristband/react-client-auth';

export function TokenTester() {
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [message, setMessage] = useState<string>('');

  /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
  const { getToken } = useWristbandToken();

  const callTokenEndpoint = async () => {
    try {
      setIsLoading(true);

      const token = await getToken();

      const response = await fetch('/api/jwt/protected', {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`
        },
      });

      if (!response.ok) {
        if ([401, 403].includes(response.status)) {
          redirectToLogin('/api/auth/login');
          window.alert('Authentication required.');
        } else {
          window.alert(`HTTP error! status: ${response.status}`);
        }
        return;
      }

      const data = await response.json();
      setMessage(JSON.stringify(data, null, 2));
    } catch (error) {
      console.log(error);
      if (error instanceof WristbandError) {
        window.alert('Authentication required.');
        redirectToLogin('/api/auth/login');
      } else {
        window.alert(`Unexpected error: ${error}`);
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <>
      <h2 className="font-bold text-lg mb-1">Token-Based Authentication Test</h2>
      <p>
        This button demonstrates token-based authentication as an alternative to cookies. The useWristbandToken() hook
        from the React SDK fetches and caches access tokens via the getToken() function. When clicked, this button sends
        the token manually in the Authorization header to the protected endpoint. The server validates the JWT token
        rather than relying on session cookies.
      </p>
      <button
        onClick={callTokenEndpoint}
        disabled={isLoading}
        className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 disabled:bg-blue-300"
      >
        {isLoading ? 'Calling...' : 'Call Token-Protected Endpoint'}
      </button>

      {message && (
        <div className="mt-4 rounded border border-gray-300 dark:border-gray-700">
          <div className="bg-gray-100 dark:bg-gray-800 p-2 border-b border-gray-300 dark:border-gray-700">
            <p className="font-bold text-sm">Response</p>
          </div>
          <div className="p-2 max-h-60 overflow-auto">
            <pre className="text-xs whitespace-pre-wrap break-all text-left">{message}</pre>
          </div>
        </div>
      )}
    </>
  );
}
