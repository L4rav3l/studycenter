import React, { useState } from 'react';

export default function ChangePassword() {
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [status, setStatus] = useState(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setStatus(null);

    if (password !== confirmPassword) {
      setStatus({ success: false, message: "Passwords don't match." });
      return;
    }

    if (password.length < 8) {
      setStatus({ success: false, message: "Password must be at least 8 characters." });
      return;
    }

    setLoading(true);

    try {
      const res = await fetch(`/api/settings/changepassword`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${localStorage.getItem('token')}`,
        },
        body: JSON.stringify({ password }),
      });

      const data = await res.json();

      if (res.ok) {
        setStatus({ success: true, message: 'Password successfully changed.' });
        setPassword('');
        setConfirmPassword('');
      } else {
        let msg = 'Something went wrong.';
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
      <h2 className="text-2xl font-semibold mb-4 text-center text-gray-800">Change Password</h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="password" className="block text-sm font-medium text-gray-700">
            New Password
          </label>
          <input
            type="password"
            id="password"
            className="mt-1 w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring focus:ring-blue-200"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>

        <div>
          <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700">
            Confirm Password
          </label>
          <input
            type="password"
            id="confirmPassword"
            className="mt-1 w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring focus:ring-blue-200"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
          />
        </div>

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-blue-600 hover:bg-blue-700 text-white font-medium py-2 px-4 rounded disabled:opacity-50"
        >
          {loading ? 'Updating...' : 'Update Password'}
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
