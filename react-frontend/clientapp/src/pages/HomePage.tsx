import { useState } from 'react';
import { useWristbandAuth } from '@wristband/react-client-auth';

import csharpLogo from '../assets/csharp.png';
import reactLogo from '../assets/react.svg';
import wristbandLogo from '../assets/wristband.png';
import { TabButton } from '../components/TabButton';
import { SessionTester } from '../components/SessionTester';
import { TokenTester } from '../components/TokenTester';

function HomePage() {
  const [activeTab, setActiveTab] = useState<'session' | 'token'>('session');

  /* WRISTBAND_TOUCHPOINT - AUTHENTICATION */
  const { isAuthenticated } = useWristbandAuth();

  return (
    <div
      className={`font-geist-sans flex flex-col items-center justify-items-center min-h-screen p-8 pt-16`}
    >
      <main className="flex flex-col gap-8 row-start-2 items-center w-full max-w-2xl">
        <div className="flex items-center gap-4">
          <a href="https://dotnet.microsoft.com/en-us/languages/csharp" target="_blank">
            <img src={csharpLogo} width={60} height={60} alt="C# logo" />
          </a>
          <a href="https://react.dev" target="_blank">
            <img src={reactLogo} width={60} height={60} alt="React logo" />
          </a>
          <a href="https://wristband.dev" target="_blank">
            <img src={wristbandLogo} width={60} height={60} alt="Wristband logo" className="animate-spin-slow" />
          </a>
        </div>

        <h1 className="text-2xl mb-1">C# + React + Wristband</h1>

        {isAuthenticated && (
          <div className="flex flex-col gap-2 w-full">
            <hr className="my-2" />
            <div className="flex border-b border-gray-200 dark:border-gray-700 mb-4">
              <TabButton
                title="Test with Session"
                isActive={activeTab === 'session'}
                onClick={() => setActiveTab('session')}
              />
              <TabButton
                title="Test with Token"
                isActive={activeTab === 'token'}
                onClick={() => setActiveTab('token')}
              />
            </div>
            <div className="flex flex-col gap-2 w-full">
              {activeTab === 'session' && <SessionTester />}
              {activeTab === 'token' && <TokenTester />}
            </div>
            <hr className="mt-6 mb-8" />
            <button
              onClick={() => window.location.href = '/api/auth/logout'}
              className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
            >
              Logout
            </button>
          </div>
        )}
      </main>
    </div>
  );
}

export { HomePage };
