import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { useNavigate, useLocation } from 'react-router-dom';

export default function Login() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState('');

  const navigate = useNavigate();
  const location = useLocation();

  // ✅ Ha van token -> átirányítás /users
  useEffect(() => {
    const storedToken = localStorage.getItem('token');
    if (storedToken) {
      navigate('/users');
    }
  }, [navigate]);

  // ✅ Ha van token query paraméterben -> mentés + átirányítás
  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const token = params.get('token');
    if (token) {
      localStorage.setItem('token', token);
      navigate('/users');
    }
  }, [location, navigate]);

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      const response = await axios.post(`/api/auth/login`, {
        Username: username,
        Password: password
      });

      if (response.data.status === 1) {
        localStorage.setItem('token', response.data.token);
        localStorage.setItem('username', username);
        navigate('/users');
      } else {
        setMessage('Login failed.');
      }
    } catch (error) {
      if (error.response) {
        switch (error.response.status) {
          case 401:
            setMessage('Unauthorized: Wrong password.');
            break;
          case 403:
            setMessage('Account inactive, check your email for confirmation.');
            break;
          case 404:
            setMessage('User not found.');
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
      <form onSubmit={handleSubmit} className="bg-white p-8 rounded shadow-md w-full max-w-sm">
        <h2 className="text-2xl font-bold mb-6 text-center">Login</h2>

        <label htmlFor="username" className="block mb-2 font-semibold">
          Username
        </label>
        <input
          id="username"
          type="text"
          value={username}
          onChange={e => setUsername(e.target.value)}
          required
          className="w-full p-2 mb-4 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-400"
        />

        <label htmlFor="password" className="block mb-2 font-semibold">
          Password
        </label>
        <input
          id="password"
          type="password"
          value={password}
          onChange={e => setPassword(e.target.value)}
          required
          className="w-full p-2 mb-6 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-400"
        />

        <button type="submit" className="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600 transition mb-4">
          Login
        </button>

        <div className="text-sm text-center">
          <a href="/magiclink" className="text-blue-500 hover:underline block">Login with Magic Link</a>
          <a href="/forgot_password" className="text-blue-500 hover:underline mt-2 block">Forgot Password?</a>
          <a href="/register" className="text-blue-500 hover:underline block">Register</a>
        </div>

        {message && <p className="text-center text-red-600 mt-4">{message}</p>}
      </form>
    </div>
  );
}
