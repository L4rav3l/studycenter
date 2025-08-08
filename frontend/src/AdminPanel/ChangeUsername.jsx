import React, { useState } from 'react';

export default function ChangeUsername() {
  const [username, setUsername] = useState('');
  const [status, setStatus] = useState(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setStatus(null);
    setLoading(true);

    try {
      const res = await fetch(`/api/settings/changeusername`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${localStorage.getItem('token')}`,
        },
        body: JSON.stringify({ username }),
      });

      const data = await res.json();

      if (res.ok) {
        setStatus({ success: true, message: 'Username successfully changed.' });
        setUsername('');
      } else {
        let msg = 'Something went wrong.';
        if (data.error === 5) msg = 'Username already taken.';
        if (data.error === 8) msg = 'Unauthorized. Please log in again.';
        setStatus({ success: false, message: msg });
      }
    } catch (err) {
      setStatus({ success: false, message: 'Network error.' });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-md mx-auto bg-white p-6 rounded-lg shadow-md mt-10">
      <h2 className="text-2xl font-semibold mb-4 text-center text-gray-800">Change Username</h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="username" className="block text-sm font-medium text-gray-700">
            New Username
          </label>
          <input
            type="text"
            id="username"
            className="mt-1 w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring focus:ring-blue-200"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />
        </div>

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded disabled:opacity-50"
        >
          {loading ? 'Updating...' : 'Update Username'}
        </button>

        {status && (
          <div
            className={`mt-4 text-center text-sm ${
              status.success ? 'text-green-600' : 'text-red-600'
            }`}
          >
            {status.message}
          </div>
        )}
      </form>
    </div>
  );
}
