import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

export default function Register() {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (token) {
      navigate('/users');
    }
  }, [navigate]);

  const handleRegister = async (e) => {
    e.preventDefault();

    try {
      const response = await axios.post(`/api/auth/register`, {
        Username: username,
        Email: email,
        Password: password
      });

      if (response.data.status === 1) {
        setMessage('Registration successful! Check your email to confirm.');
      }
    } catch (error) {
      if (error.response) {
        switch (error.response.status) {
          case 409:
            switch (error.response.data.error) {
              case 4:
                setMessage('Invalid email format.');
                break;
              case 5:
                setMessage('Username already exists.');
                break;
              case 6:
                setMessage('Email already registered.');
                break;
              default:
                setMessage('Conflict error.');
            }
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
      <form onSubmit={handleRegister} className="bg-white p-8 rounded shadow-md w-full max-w-md">
        <h2 className="text-2xl font-bold mb-6 text-center">Register</h2>

        <label htmlFor="username" className="block mb-2 font-semibold">Username</label>
        <input
          id="username"
          type="text"
          value={username}
          onChange={e => setUsername(e.target.value)}
          required
          className="w-full p-2 mb-4 border border-gray-300 rounded"
        />

        <label htmlFor="email" className="block mb-2 font-semibold">Email</label>
        <input
          id="email"
          type="email"
          value={email}
          onChange={e => setEmail(e.target.value)}
          required
          className="w-full p-2 mb-4 border border-gray-300 rounded"
        />

        <label htmlFor="password" className="block mb-2 font-semibold">Password</label>
        <input
          id="password"
          type="password"
          value={password}
          onChange={e => setPassword(e.target.value)}
          required
          className="w-full p-2 mb-6 border border-gray-300 rounded"
        />

        <button
          type="submit"
          className="w-full bg-blue-500 text-white py-2 rounded hover:bg-blue-600 transition"
        >
          Register
        </button>

                <div className="text-sm text-center">
                  <a href="/login" className="text-blue-500 hover:underline block">Login</a>
                </div>

        {message && <p className="mt-4 text-center text-red-600">{message}</p>}
      </form>
    </div>
  );
}
