// components/AdminLayout.jsx
import React, { useState } from 'react';
import { Link, Outlet, useLocation } from 'react-router-dom';
import useAuthCheck from '../hooks/useAuthCheck';
import {
  LayoutDashboard,
  FileText,
  Calendar,
  CheckSquare,
  BookText,
  ChevronDown,
  ChevronUp,
  Settings,
} from 'lucide-react';

export default function AdminLayout() {
  useAuthCheck();

  const { pathname } = useLocation();
  const [open, setOpen] = useState({
    documents: false,
    settings: false,
  });

  const toggle = (key) => {
    setOpen((prev) => ({ ...prev, [key]: !prev[key] }));
  };

  return (
    <div className="flex min-h-screen bg-gray-100">
      {/* Sidebar */}
      <aside className="w-64 bg-white border-r shadow-sm flex flex-col p-4">
        <div className="text-2xl font-bold text-blue-600 mb-8 text-center">Study Center</div>

        <nav className="flex flex-col gap-2 text-sm font-medium">
          <Link
            to="/users"
            className={`flex items-center gap-2 px-4 py-2 rounded-lg transition ${
              pathname === '/users'
                ? 'bg-blue-100 text-blue-700 font-semibold'
                : 'text-gray-700 hover:bg-gray-100'
            }`}
          >
            <LayoutDashboard size={18} />
            Dashboard
          </Link>

          {/* Document Manager dropdown */}
          <button
            onClick={() => toggle('documents')}
            className={`flex items-center justify-between px-4 py-2 rounded-lg transition ${
              pathname.startsWith('/users/documents')
                ? 'bg-blue-100 text-blue-700 font-semibold'
                : 'text-gray-700 hover:bg-gray-100'
            }`}
          >
            <div className="flex items-center gap-2">
              <FileText size={18} />
              Document Manager
            </div>
            {open.documents ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
          </button>
          {open.documents && (
            <div className="ml-6 flex flex-col gap-1 mt-1">
              <Link
                to="/users/documents/main-category"
                className={`block px-3 py-1 rounded ${
                  pathname === '/users/documents/main-category'
                    ? 'text-blue-700 font-semibold'
                    : 'text-gray-600 hover:text-gray-800'
                }`}
              >
                Main Category Manage
              </Link>
            </div>
          )}

          <Link
            to="/users/calendar"
            className={`flex items-center gap-2 px-4 py-2 rounded-lg transition ${
              pathname === '/users/calendar'
                ? 'bg-blue-100 text-blue-700 font-semibold'
                : 'text-gray-700 hover:bg-gray-100'
            }`}
          >
            <Calendar size={18} />
            Calendar
          </Link>

          <Link
            to="/users/todo"
            className={`flex items-center gap-2 px-4 py-2 rounded-lg transition ${
              pathname === '/users/todo'
                ? 'bg-blue-100 text-blue-700 font-semibold'
                : 'text-gray-700 hover:bg-gray-100'
            }`}
          >
            <CheckSquare size={18} />
            Todo
          </Link>

          <Link
            to="/users/wordstudy"
            className={`flex items-center gap-2 px-4 py-2 rounded-lg transition ${
              pathname === '/users/wordstudy'
                ? 'bg-blue-100 text-blue-700 font-semibold'
                : 'text-gray-700 hover:bg-gray-100'
            }`}
          >
            <BookText size={18} />
            WordStudy
          </Link>

          {/* Settings dropdown */}
          <button
            onClick={() => toggle('settings')}
            className={`flex items-center justify-between px-4 py-2 rounded-lg transition ${
              pathname.startsWith('/users/settings')
                ? 'bg-blue-100 text-blue-700 font-semibold'
                : 'text-gray-700 hover:bg-gray-100'
            }`}
          >
            <div className="flex items-center gap-2">
              <Settings size={18} />
              Settings
            </div>
            {open.settings ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
          </button>
          {open.settings && (
            <div className="ml-6 flex flex-col gap-1 mt-1">
              <Link
                to="/users/settings/change-username"
                className={`block px-3 py-1 rounded ${
                  pathname === '/users/settings/change-username'
                    ? 'text-blue-700 font-semibold'
                    : 'text-gray-600 hover:text-gray-800'
                }`}
              >
                Change Username
              </Link>
              <Link
                to="/users/settings/change-password"
                className={`block px-3 py-1 rounded ${
                  pathname === '/users/settings/change-password'
                    ? 'text-blue-700 font-semibold'
                    : 'text-gray-600 hover:text-gray-800'
                }`}
              >
                Change Password
              </Link>
              <Link
                to="/users/settings/change-email"
                className={`block px-3 py-1 rounded ${
                  pathname === '/users/settings/change-email'
                    ? 'text-blue-700 font-semibold'
                    : 'text-gray-600 hover:text-gray-800'
                }`}
              >
                Change Email
              </Link>
            </div>
          )}
        </nav>
      </aside>

      {/* Main content */}
      <main className="flex-1 p-8">
        <Outlet />
      </main>
    </div>
  );
}
