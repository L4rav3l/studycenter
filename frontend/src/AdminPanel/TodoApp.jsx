import React, { useEffect, useState, Fragment } from "react";
import axios from "axios";
import dayjs from "dayjs";
import { Menu, Transition } from "@headlessui/react";

const API_URL = process.env.REACT_APP_API_URL;

const TodoApp = () => {
  const [todos, setTodos] = useState([]);
  const [date, setDate] = useState(dayjs().format("YYYY-MM-DD"));
  const [newTask, setNewTask] = useState("");

  const token = localStorage.getItem("token");
  const headers = {
    Authorization: `Bearer ${token}`,
  };

  const fetchTodos = async (selectedDate) => {
    try {
      const response = await axios.get(
        `${API_URL}/api/todo/get?Date=${selectedDate}`,
        { headers }
      );
      setTodos(response.data.items);
    } catch (error) {
      console.error("Error fetching todos:", error);
    }
  };

  const createTodo = async () => {
    if (!newTask.trim()) return;
    try {
      await axios.post(
        `/api/todo/create`,
        {
          Name: newTask,
          Date: date,
        },
        { headers }
      );
      setNewTask("");
      fetchTodos(date);
    } catch (error) {
      console.error("Error creating todo:", error);
    }
  };

  const deleteTodo = async (id) => {
    try {
      await axios.post(
        `/api/todo/delete`,
        { Id: id },
        { headers }
      );
      fetchTodos(date);
    } catch (error) {
      console.error("Error deleting todo:", error);
    }
  };

  const updateStatus = async (id, status) => {
    try {
      await axios.post(
        `/api/todo/status`,
        { Id: id, Status: status },
        { headers }
      );
      fetchTodos(date);
    } catch (error) {
      console.error("Error updating status:", error);
    }
  };

  const editTodo = async (id, name) => {
    try {
      await axios.post(
        `/api/todo/edit`,
        {
          Id: id,
          Name: name,
          Date: date,
        },
        { headers }
      );
      fetchTodos(date);
    } catch (error) {
      console.error("Error editing todo:", error);
    }
  };

  const changeDate = (offsetDays) => {
    const newDate = dayjs(date).add(offsetDays, "day").format("YYYY-MM-DD");
    setDate(newDate);
  };

  useEffect(() => {
    fetchTodos(date);
  }, [date]);

  return (
    <div className="min-h-screen bg-gray-100 p-6">
      <div className="max-w-xl mx-auto bg-white shadow-md rounded-lg p-6 space-y-6">
        <h1 className="text-3xl font-bold text-center text-gray-800">To-Do List</h1>

        <div className="flex items-center gap-2">
          <button
            onClick={() => changeDate(-1)}
            className="px-3 py-2 bg-gray-200 rounded hover:bg-gray-300"
          >
            ◀
          </button>
          <input
            type="date"
            value={date}
            onChange={(e) => setDate(e.target.value)}
            className="flex-1 border border-gray-300 rounded px-4 py-2 text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-400"
          />
          <button
            onClick={() => changeDate(1)}
            className="px-3 py-2 bg-gray-200 rounded hover:bg-gray-300"
          >
            ▶
          </button>
        </div>

        <div className="flex gap-2">
          <input
            type="text"
            placeholder="New task..."
            value={newTask}
            onChange={(e) => setNewTask(e.target.value)}
            className="flex-1 border border-gray-300 rounded px-4 py-2 text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-400"
          />
          <button
            onClick={createTodo}
            className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded shadow"
          >
            Add
          </button>
        </div>

        <div className="space-y-3">
          {todos.map((todo) => (
            <div
              key={todo.ID}
              className="bg-gray-50 border border-gray-300 rounded p-4 flex items-center gap-4 justify-between shadow-sm"
            >
              <div className="flex items-center gap-4 flex-1">
                <input
                  type="checkbox"
                  checked={todo.STATUS}
                  onChange={() => updateStatus(todo.ID, !todo.STATUS)}
                  className="w-5 h-5 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                />
                <span className="text-gray-800">{todo.NAME}</span>
              </div>
              <Menu as="div" className="relative inline-block text-left">
                <Menu.Button className="inline-flex justify-center w-8 h-8 text-gray-600 hover:text-gray-900">
                  <span className="text-xl font-bold">⋮</span>
                </Menu.Button>
                <Transition
                  as={Fragment}
                  enter="transition ease-out duration-100"
                  enterFrom="transform opacity-0 scale-95"
                  enterTo="transform opacity-100 scale-100"
                  leave="transition ease-in duration-75"
                  leaveFrom="transform opacity-100 scale-100"
                  leaveTo="transform opacity-0 scale-95"
                >
                  <Menu.Items className="absolute right-0 z-10 mt-2 w-36 origin-top-right rounded-md bg-white shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none">
                    <div className="py-1">
                      <Menu.Item>
                        {({ active }) => (
                          <button
                            onClick={() => {
                              const newName = prompt("Edit task:", todo.NAME);
                              if (newName !== null) editTodo(todo.ID, newName);
                            }}
                            className={`$${active ? "bg-gray-100" : ""} block w-full text-left px-4 py-2 text-sm text-gray-700`}
                          >
                            Edit
                          </button>
                        )}
                      </Menu.Item>
                      <Menu.Item>
                        {({ active }) => (
                          <button
                            onClick={() => deleteTodo(todo.ID)}
                            className={`$${active ? "bg-red-100 text-red-700" : "text-red-600"} block w-full text-left px-4 py-2 text-sm`}
                          >
                            Delete
                          </button>
                        )}
                      </Menu.Item>
                    </div>
                  </Menu.Items>
                </Transition>
              </Menu>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default TodoApp;
