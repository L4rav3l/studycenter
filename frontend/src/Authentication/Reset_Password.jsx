import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { useSearchParams, useNavigate } from 'react-router-dom';

export default function ResetPassword() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [message, setMessage] = useState('');
  const [tokenValid, setTokenValid] = useState(false);
  const [loading, setLoading] = useState(true);

  const token = searchParams.get('token');

  useEffect(() => {
    if (!token) {
      navigate('/auth/forgot_password');
      return;
    }

    axios.post(`/api/auth/verifytoken`, { token })
      .then(res => {
        if (res.data.status === 1) {
          setTokenValid(true);
        } else {
          navigate('/forgot_password');
        }
      })
      .catch(() => {
        navigate('/forgot_password');
      })
      .finally(() => setLoading(false));
  }, [token, navigate]);

  const handleReset = async (e) => {
    e.preventDefault();

    if (password !== confirmPassword) {
      setMessage("Passwords do not match.");
      return;
    }

    try {
      const response = await axios.post(`${process.env.REACT_APP_API_URL}/api/auth/reset-password`, {
        token,
        password
      });

      if (response.data.status === 1) {
        setMessage("Password successfully reset. You can now log in.");
        setTimeout(() => navigate('/auth/login'), 3000);
      }
    } catch (error) {
      setMessage("Something went wrong.");
    }
  };

  if (loading) return <div className="text-center mt-10">Verifying token...</div>;

  if (!tokenValid) return null;

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100">
      <form onSubmit={handleReset} className="bg-white p-8 rounded shadow-md w-full max-w-sm">
        <h2 className="text-2xl font-bold mb-6 text-center">Reset Password</h2>

        <label className="block mb-2 font-semibold">New Password</label>
        <input
          type="password"
          value={password}
          onChange={e => setPassword(e.target.value)}
          required
          className="w-full p-2 mb-4 border border-gray-300 rounded"
        />

        <label className="block mb-2 font-semibold">Confirm Password</label>
        <input
          type="password"
          value={confirmPassword}
          onChange={e => setConfirmPassword(e.target.value)}
          required
          className="w-full p-2 mb-6 border border-gray-300 rounded"
        />

        <button
          type="submit"
          className="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600 transition"
        >
          Reset Password
        </button>

        {message && <p className="mt-4 text-center text-blue-600">{message}</p>}
      </form>
    </div>
  );
}
