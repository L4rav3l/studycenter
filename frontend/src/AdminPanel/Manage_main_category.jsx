import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { MoreVertical, Edit, Trash2, Plus } from 'lucide-react';
import AdminLayout from '../components/AdminLayout';
import { useNavigate } from 'react-router-dom';

const API_URL = process.env.REACT_APP_API_URL;

export default function MainCategoryManager() {
  const [categories, setCategories] = useState([]);
  const [newCategory, setNewCategory] = useState('');
  const [editing, setEditing] = useState(null);
  const [dropdownOpen, setDropdownOpen] = useState(null);
  const [renameValue, setRenameValue] = useState('');
  const [confirmDelete, setConfirmDelete] = useState(null);
  const [confirmName, setConfirmName] = useState('');

  const navigate = useNavigate();

  useEffect(() => {
    fetchCategories();
  }, []);

  async function fetchCategories() {
    try {
      const token = localStorage.getItem('token');
      const res = await axios.get(`/api/graphs/main/get`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      setCategories(res.data.items);
    } catch (err) {
      if (err.response?.status === 401) {
        localStorage.removeItem('token');
        window.location.href = '/login';
      }
    }
  }

  async function createCategory() {
    if (!newCategory) return;
    try {
      const token = localStorage.getItem('token');
      await axios.post(
        `/api/graphs/main/create`,
        { name: newCategory },
        { headers: { Authorization: `Bearer ${token}` } }
      );
      setNewCategory('');
      fetchCategories();
    } catch (err) {
      console.error(err);
    }
  }

  async function renameCategory(id) {
    try {
      const token = localStorage.getItem('token');
      await axios.post(
        `/api/graphs/main/rename`,
        { id, name: renameValue },
        { headers: { Authorization: `Bearer ${token}` } }
      );
      setEditing(null);
      setRenameValue('');
      fetchCategories();
    } catch (err) {
      console.error(err);
    }
  }

  async function deleteCategory(id, name) {
    if (confirmName !== name) return;
    try {
      const token = localStorage.getItem('token');
      await axios.post(
        `/api/graphs/main/delete`,
        { id },
        { headers: { Authorization: `Bearer ${token}` } }
      );
      setConfirmDelete(null);
      setConfirmName('');
      fetchCategories();
    } catch (err) {
      console.error(err);
    }
  }

  function enterCategory(id) {
    navigate(`/users/documents/main-category/${id}`);
  }

  return (
    <div className="max-w-2xl mx-auto p-6 bg-white rounded-xl shadow-md">
      <h1 className="text-2xl font-semibold mb-6">Main Category Manager</h1>

      <div className="flex items-center gap-2 mb-4">
        <input
          type="text"
          placeholder="New main category"
          value={newCategory}
          onChange={(e) => setNewCategory(e.target.value)}
          className="border rounded px-3 py-2 w-full"
        />
        <button onClick={createCategory} className="bg-blue-600 text-white px-4 py-2 rounded">
          <Plus size={16} />
        </button>
      </div>

      <ul className="space-y-3">
        {categories.map((cat) => (
          <li key={cat.ID} className="flex justify-between items-center border p-3 rounded">
            {editing === cat.ID ? (
              <input
                value={renameValue}
                onChange={(e) => setRenameValue(e.target.value)}
                className="border px-2 py-1 rounded w-full mr-2"
              />
            ) : (
              <span
                onClick={() => enterCategory(cat.ID)}
                className="font-medium cursor-pointer text-blue-600 hover:underline"
                title="Enter category"
              >
                {cat.NAME}
              </span>
            )}

            <div className="relative">
              <button
                onClick={() => setDropdownOpen(dropdownOpen === cat.ID ? null : cat.ID)}
                className="text-gray-600 hover:text-black"
              >
                <MoreVertical size={20} />
              </button>
              {dropdownOpen === cat.ID && (
                <div className="absolute right-0 mt-2 bg-white border rounded shadow z-10 w-32">
                  <button
                    onClick={() => {
                      setEditing(cat.ID);
                      setRenameValue(cat.NAME);
                      setDropdownOpen(null);
                    }}
                    className="w-full text-left px-3 py-2 hover:bg-gray-100 flex items-center gap-2"
                  >
                    <Edit size={14} /> Rename
                  </button>
                  <button
                    onClick={() => {
                      setConfirmDelete(cat);
                      setDropdownOpen(null);
                    }}
                    className="w-full text-left px-3 py-2 hover:bg-gray-100 flex items-center gap-2 text-red-600"
                  >
                    <Trash2 size={14} /> Delete
                  </button>
                </div>
              )}
            </div>

            {editing === cat.ID && (
              <button
                onClick={() => renameCategory(cat.ID)}
                className="ml-2 text-sm text-blue-600 hover:underline"
              >
                Save
              </button>
            )}
          </li>
        ))}
      </ul>

      {confirmDelete && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
          <div className="bg-white p-6 rounded shadow-lg max-w-md w-full">
            <h2 className="text-lg font-semibold mb-4">Confirm Delete</h2>
            <p className="mb-2">
              Please type the category name: <strong>{confirmDelete.NAME}</strong> to confirm deletion.
            </p>
            <input
              type="text"
              className="w-full border px-3 py-2 rounded mb-4"
              value={confirmName}
              onChange={(e) => setConfirmName(e.target.value)}
            />
            <div className="flex justify-end gap-2">
              <button
                className="px-4 py-2 rounded bg-gray-200"
                onClick={() => setConfirmDelete(null)}
              >
                Cancel
              </button>
              <button
                className="px-4 py-2 rounded bg-red-600 text-white"
                onClick={() => deleteCategory(confirmDelete.ID, confirmDelete.NAME)}
              >
                Delete
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
