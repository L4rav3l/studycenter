import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter, Routes, Route } from 'react-router-dom';

import Login from './Authentication/Login';
import Register from './Authentication/Register';
import MagicLink from './Authentication/MagicLink';
import ForgotPassword from './Authentication/Forgot_Password';
import ResetPassword from './Authentication/Reset_Password';
import AdminLayout from './components/AdminLayout';
import MainCategoryManager from './AdminPanel/Manage_main_category';
import GraphPage from './AdminPanel/Manage_tree';
import CalendarPage from './AdminPanel/Calendar';
import TodoApp from './AdminPanel/TodoApp';
import QuizletClone from './AdminPanel/WordStudy';
import ChangeEmail from './AdminPanel/ChangeEmail';
import ChangePassword from './AdminPanel/ChangePassword';
import ChangeUsername from './AdminPanel/ChangeUsername';
import Dashboard from './AdminPanel/Dashboard';
import VerifyPage from './Authentication/Verify';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <React.StrictMode>
    <BrowserRouter>
      <Routes>
        <Route path="" element={<Login />} /> 
        <Route path="magiclink" element={<MagicLink />} />
        <Route path="login" element={<Login />} />
        <Route path="register" element={<Register />} />
        <Route path="forgot_password" element={<ForgotPassword />} />
        <Route path="reset_password" element={<ResetPassword />} />
        <Route path="register/verify" element={<VerifyPage />} />

        {/* Admin routes */}
        <Route path="users" element={<AdminLayout />}>
          <Route path="" element={<Dashboard />} />
          <Route path="documents/main-category" element={<MainCategoryManager />} />
          <Route path="documents/main-category/:id/*" element={<GraphPage />}/>
          <Route path="calendar" element={<CalendarPage />} />
          <Route path="todo" element={<TodoApp />} />
          <Route path="wordstudy" element={<QuizletClone />} />
          <Route path="settings/change-email" element={<ChangeEmail/>} />
          <Route path="settings/change-password" element={<ChangePassword />} />
          <Route path="settings/change-username" element={< ChangeUsername/>} />
        </Route>
      </Routes>
    </BrowserRouter>
  </React.StrictMode>
);
