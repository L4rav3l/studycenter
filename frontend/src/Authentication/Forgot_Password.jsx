import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

export default function ForgotPassword() {
  const [email, setEmail] = useState('');
  const [message, setMessage] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (token) {
      navigate('/users');
    }
  }, [navigate]);

  const handleForgotPassword = async (e) => {
    e.preventDefault();

    try {
      const response = await axios.post(`/api/auth/forgot-password`, {
        Email: email,
      });

      if (response.data.status === 1) {
        setMessage('Password reset link sent to your email.');
      }
    } catch (error) {
      if (error.response) {
        switch (error.response.status) {
          case 404:
            setMessage('User not found.');
            break;
          case 409:
            setMessage('Invalid email format.');
            break;
          default:
            setMessage('Server error.');
        }
      } else {
        setMessage('Network error.');
      }
    }
  };

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100">
      <form onSubmit={handleForgotPassword} className="bg-white p-8 rounded shadow-md w-full max-w-sm">
        <h2 className="text-2xl font-bold mb-6 text-center">Forgot Password</h2>

        <label htmlFor="email" className="block mb-2 font-semibold">
          Email
        </label>
        <input
          id="email"
          type="email"
          value={email}
          onChange={e => setEmail(e.target.value)}
          required
          className="w-full p-2 mb-6 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-400"
        />

        <button type="submit" className="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600 transition">
          Send Reset Link
        </button>

        {message && <p className="text-center text-blue-600 mt-4">{message}</p>}
      </form>
    </div>
  );
}
