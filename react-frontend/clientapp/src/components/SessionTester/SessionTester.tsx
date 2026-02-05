import { useState } from 'react';
import { isAxiosError } from 'axios';
import { redirectToLogin } from '@wristband/react-client-auth';

import { backendApiClient } from '../../api/backend-api-client';

export function SessionTester() {
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [result, setResult] = useState<string>('');

  const callProtectedEndpoint = async () => {
    try {
      setIsLoading(true);
      const response = await backendApiClient.get('/api/session/protected');
      const data = response.data as { message: string, value: number };
      setResult(JSON.stringify(data, null, 2));
    } catch (error) {
      handleApiError(error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleApiError = (error: unknown) => {
    console.error(error);
    setResult('');

    if (isAxiosError(error) && error.response && [401, 403].includes(error.response.status)) {
      redirectToLogin('/api/auth/login');
      window.alert('Authentication required.');
    } else {
      window.alert(`Error: ${error}`);
    }
  };

  return (
    <>
      <h2 className="font-bold text-lg mb-1">Session-Based Authentication Test</h2>
      <p>
        This button demonstrates cookie-based authentication for API calls. When clicked, the browser automatically
        sends the session cookie to the ASP.NET server. The server's RequireWristbandSession() validates
        the session cookie before allowing access to protected resources.
      </p>
      <button
        onClick={callProtectedEndpoint}
        disabled={isLoading}
        className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 disabled:bg-blue-300"
      >
        {isLoading ? 'Calling...' : 'Call Protected Endpoint'}
      </button>

      {result && (
        <div className="mt-4 rounded border border-gray-300 dark:border-gray-700">
          <div className="bg-gray-100 dark:bg-gray-800 p-2 border-b border-gray-300 dark:border-gray-700">
            <p className="font-bold text-sm">Response</p>
          </div>
          <div className="p-2 max-h-60 overflow-auto">
            <pre className="text-xs whitespace-pre-wrap break-all text-left">{result}</pre>
          </div>
        </div>
      )}
    </>
  );
}
