import React, { useState, useEffect } from 'react';
import axios from 'axios';


axios.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

const QuizletClone = () => {
  // State for application data
  const [items, setItems] = useState([]);
  const [currentFolder, setCurrentFolder] = useState(null);
  const [currentSet, setCurrentSet] = useState(null);
  const [flashcards, setFlashcards] = useState([]);
  const [mode, setMode] = useState('browse'); // browse, learn, test, edit
  const [currentCardIndex, setCurrentCardIndex] = useState(0);
  const [isFlipped, setIsFlipped] = useState(false);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [modalType, setModalType] = useState('folder'); // folder or set
  const [editData, setEditData] = useState(null);
  const [formData, setFormData] = useState({
    name: '',
    notification_date: '',
    end_date: '',
    folder_id: null,
    front: '',
    back: ''
  });

  // Fetch all items (folders and sets)
  const fetchItems = async () => {
    try {
      const response = await axios.get('/api/wordstudy/itemlist');
      setItems(response.data.items);
    } catch (error) {
      console.error('Error fetching items:', error);
    }
  };

  // Fetch flashcards for a set
  const fetchFlashcards = async (setId) => {
    try {
      const response = await axios.get(`/api/wordstudy/sett/get?Id=${setId}`);
      setFlashcards(response.data.items);
    } catch (error) {
      console.error('Error fetching flashcards:', error);
    }
  };

  // Handle folder click
  const handleFolderClick = (folder) => {
    setCurrentFolder(folder);
    setCurrentSet(null);
  };

  // Handle set click
  const handleSetClick = async (set) => {
    setCurrentSet(set);
    setCurrentFolder(null);
    await fetchFlashcards(set.ID);
    setMode('browse');
  };

  // Handle back to root
  const handleBackToRoot = () => {
    setCurrentFolder(null);
    setCurrentSet(null);
  };

  // Handle create new item
  const handleCreate = async (e) => {
    e.preventDefault();
    try {
      if (modalType === 'folder') {
        await axios.post('/api/wordstudy/folder/create', {
          Name: formData.name,
          Notification_date: formData.notification_date || null,
          End_date: formData.end_date || null
        });
      } else {
        await axios.post('/api/wordstudy/sett/create', {
          Name: formData.name,
          Folder: formData.folder_id || null,
          Notification_date: formData.notification_date || null,
          End_date: formData.end_date || null
        });
      }
      setShowCreateModal(false);
      setFormData({
        name: '',
        notification_date: '',
        end_date: '',
        folder_id: null,
        front: '',
        back: ''
      });
      fetchItems();
    } catch (error) {
      console.error('Error creating item:', error);
    }
  };

  // Handle edit item
  const handleEdit = async (e) => {
    e.preventDefault();
    try {
      if (modalType === 'folder') {
        await axios.post('/api/wordstudy/folder/edit', {
          Id: editData.ID,
          Name: formData.name,
          Notification_date: formData.notification_date || null,
          End_date: formData.end_date || null
        });
      } else {
        await axios.post('/api/wordstudy/sett/edit', {
          Id: editData.ID,
          Name: formData.name,
          Notification_date: formData.notification_date || null,
          End_date: formData.end_date || null
        });
      }
      setShowEditModal(false);
      fetchItems();
    } catch (error) {
      console.error('Error editing item:', error);
    }
  };

  // Handle delete item
  const handleDelete = async (item) => {
    try {
      if (item.TYPE === 'FOLDER') {
        await axios.post('/api/wordstudy/folder/delete', { Id: item.ID });
      } else {
        await axios.post('/api/wordstudy/sett/delete', { Id: item.ID });
      }
      fetchItems();
    } catch (error) {
      console.error('Error deleting item:', error);
    }
  };

  // Handle move set to folder
  const handleMoveSet = async (setId, folderId) => {
    try {
      await axios.post('/api/wordstudy/sett/setparent', {
        Folder_id: folderId,
        Sett_id: setId
      });
      fetchItems();
    } catch (error) {
      console.error('Error moving set:', error);
    }
  };

  // Handle create flashcard
  const handleCreateFlashcard = async (e) => {
    e.preventDefault();
    try {
      await axios.post('/api/wordstudy/flashcard/create', {
        Sett_id: currentSet.ID,
        Front: formData.front,
        Back: formData.back
      });
      setFormData({ ...formData, front: '', back: '' });
      fetchFlashcards(currentSet.ID);
    } catch (error) {
      console.error('Error creating flashcard:', error);
    }
  };

  // Handle edit flashcard
  const handleEditFlashcard = async (e) => {
    e.preventDefault();
    try {
      await axios.post('/api/wordstudy/flashcard/edit', {
        Id: editData.ID,
        Front: formData.front,
        Back: formData.back
      });
      setShowEditModal(false);
      fetchFlashcards(currentSet.ID);
    } catch (error) {
      console.error('Error editing flashcard:', error);
    }
  };

  // Handle delete flashcard
  const handleDeleteFlashcard = async (flashcardId) => {
    try {
      await axios.post('/api/wordstudy/flashcard/delete', { Id: flashcardId });
      fetchFlashcards(currentSet.ID);
    } catch (error) {
      console.error('Error deleting flashcard:', error);
    }
  };

  // Study mode functions
  const nextCard = () => {
    setIsFlipped(false);
    setCurrentCardIndex((prevIndex) => 
      prevIndex < flashcards.length - 1 ? prevIndex + 1 : 0
    );
  };

  const prevCard = () => {
    setIsFlipped(false);
    setCurrentCardIndex((prevIndex) => 
      prevIndex > 0 ? prevIndex - 1 : flashcards.length - 1
    );
  };

  // Filter items based on current folder
  const filteredItems = items.filter(item => {
    if (currentFolder) {
      return item.TYPE === 'SETT' && item.FOLDER_ID === currentFolder.ID;
    } else if (currentSet) {
      return false; // We're viewing a set's flashcards
    } else {
      return item.TYPE === 'FOLDER' || (item.TYPE === 'SETT' && !item.FOLDER_ID);
    }
  });

  // Initialize
  useEffect(() => {
    fetchItems();
  }, []);

  return (
    <div className="min-h-screen bg-gray-100">
      {/* Navbar */}
      <nav className="bg-blue-600 text-white p-4 shadow-md">
        <div className="container mx-auto flex justify-between items-center">
          <h1 className="text-2xl font-bold">WordStudy</h1>
          <div className="flex space-x-4">
            <button 
              onClick={() => setMode('browse')} 
              className={`px-4 py-2 rounded ${mode === 'browse' ? 'bg-blue-800' : 'hover:bg-blue-700'}`}
            >
              Browse
            </button>
            {currentSet && (
              <>
                <button 
                  onClick={() => setMode('learn')} 
                  className={`px-4 py-2 rounded ${mode === 'learn' ? 'bg-blue-800' : 'hover:bg-blue-700'}`}
                >
                  Learn
                </button>
                <button 
                  onClick={() => setMode('test')} 
                  className={`px-4 py-2 rounded ${mode === 'test' ? 'bg-blue-800' : 'hover:bg-blue-700'}`}
                >
                  Test
                </button>
              </>
            )}
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <div className="container mx-auto p-4">
        {/* Breadcrumb */}
        <div className="flex items-center mb-4">
          <button 
            onClick={handleBackToRoot} 
            className="text-blue-600 hover:underline mr-2"
          >
            Home
          </button>
          {currentFolder && (
            <>
              <span className="mx-2">/</span>
              <span className="font-medium">{currentFolder.NAME}</span>
            </>
          )}
          {currentSet && (
            <>
              <span className="mx-2">/</span>
              <span className="font-medium">{currentSet.NAME}</span>
            </>
          )}
        </div>

        {/* Browse Mode */}
        {mode === 'browse' && (
          <div>
            {/* Action Buttons */}
            <div className="flex space-x-4 mb-6">
              <button 
                onClick={() => {
                  setModalType('folder');
                  setShowCreateModal(true);
                }}
                className="bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600"
              >
                New Folder
              </button>
              <button 
                onClick={() => {
                  setModalType('set');
                  setShowCreateModal(true);
                }}
                className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
              >
                New Set
              </button>
            </div>

            {/* Items Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {filteredItems.map((item) => (
                <div 
                  key={`${item.TYPE}-${item.ID}`} 
                  className="bg-white p-4 rounded-lg shadow hover:shadow-md transition-shadow"
                >
                  <div className="flex justify-between items-start">
                    <div 
                      onClick={() => item.TYPE === 'FOLDER' ? handleFolderClick(item) : handleSetClick(item)}
                      className="cursor-pointer flex-1"
                    >
                      <h3 className="text-lg font-semibold">{item.NAME}</h3>
                      <p className="text-gray-500 text-sm">
                        {item.TYPE === 'FOLDER' ? 'Folder' : 'Flashcard Set'}
                      </p>
                      <p className="text-gray-500 text-sm">
                        Created: {new Date(item.DATE).toLocaleDateString()}
                      </p>
                    </div>
                    <div className="flex space-x-2">
                      <button 
                        onClick={() => {
                          setModalType(item.TYPE === 'FOLDER' ? 'folder' : 'set');
                          setEditData(item);
                          setFormData({
                            name: item.NAME,
                            notification_date: '',
                            end_date: '',
                            folder_id: item.FOLDER_ID || null
                          });
                          setShowEditModal(true);
                        }}
                        className="text-blue-500 hover:text-blue-700"
                      >
                        Edit
                      </button>
                      <button 
                        onClick={() => handleDelete(item)}
                        className="text-red-500 hover:text-red-700"
                      >
                        Delete
                      </button>
                      {item.TYPE === 'SETT' && (
                        <select 
                          onChange={(e) => handleMoveSet(item.ID, e.target.value === 'null' ? null : parseInt(e.target.value))}
                          value={item.FOLDER_ID || 'null'}
                          className="border rounded px-2 py-1 text-sm"
                        >
                          <option value="null">Root</option>
                          {items.filter(i => i.TYPE === 'FOLDER').map(folder => (
                            <option key={`folder-${folder.ID}`} value={folder.ID}>
                              {folder.NAME}
                            </option>
                          ))}
                        </select>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>

            {/* Current Set Flashcards */}
            {currentSet && (
              <div className="mt-8">
                <h2 className="text-xl font-bold mb-4">Flashcards in {currentSet.NAME}</h2>
                <div className="mb-4">
                  <form onSubmit={handleCreateFlashcard} className="flex space-x-4">
                    <input
                      type="text"
                      placeholder="Front"
                      value={formData.front}
                      onChange={(e) => setFormData({ ...formData, front: e.target.value })}
                      className="border rounded px-3 py-2 flex-1"
                      required
                    />
                    <input
                      type="text"
                      placeholder="Back"
                      value={formData.back}
                      onChange={(e) => setFormData({ ...formData, back: e.target.value })}
                      className="border rounded px-3 py-2 flex-1"
                      required
                    />
                    <button 
                      type="submit"
                      className="bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600"
                    >
                      Add Card
                    </button>
                  </form>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {flashcards.map((card) => (
                    <div key={card.ID} className="bg-white p-4 rounded-lg shadow">
                      <div className="flex justify-between">
                        <div>
                          <p className="font-medium">Front: {card.FRONT}</p>
                          <p className="font-medium">Back: {card.BACK}</p>
                        </div>
                        <div className="flex space-x-2">
                          <button 
                            onClick={() => {
                              setEditData(card);
                              setFormData({
                                front: card.FRONT,
                                back: card.BACK
                              });
                              setShowEditModal(true);
                            }}
                            className="text-blue-500 hover:text-blue-700"
                          >
                            Edit
                          </button>
                          <button 
                            onClick={() => handleDeleteFlashcard(card.ID)}
                            className="text-red-500 hover:text-red-700"
                          >
                            Delete
                          </button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>
        )}

        {/* Learn Mode */}
        {mode === 'learn' && currentSet && flashcards.length > 0 && (
          <div className="flex flex-col items-center">
            <div 
              className={`w-full max-w-md h-64 rounded-xl shadow-lg cursor-pointer mb-6 flex items-center justify-center p-6 text-center transition-all duration-500 ${isFlipped ? 'bg-blue-100' : 'bg-white'}`}
              onClick={() => setIsFlipped(!isFlipped)}
            >
              <p className="text-xl font-medium">
                {isFlipped ? flashcards[currentCardIndex].BACK : flashcards[currentCardIndex].FRONT}
              </p>
            </div>
            <div className="flex space-x-4">
              <button 
                onClick={prevCard}
                className="bg-gray-200 px-4 py-2 rounded hover:bg-gray-300"
              >
                Previous
              </button>
              <button 
                onClick={nextCard}
                className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
              >
                Next
              </button>
            </div>
            <div className="mt-4">
              <p>{currentCardIndex + 1} of {flashcards.length}</p>
            </div>
          </div>
        )}

        {/* Test Mode */}
        {mode === 'test' && currentSet && flashcards.length > 0 && (
          <div className="flex flex-col items-center">
            <div className="w-full max-w-md mb-6">
              <p className="text-xl font-medium mb-4">{flashcards[currentCardIndex].FRONT}</p>
              <input
                type="text"
                placeholder="Your answer..."
                className="border rounded px-3 py-2 w-full mb-4"
              />
              <button 
                onClick={() => setIsFlipped(!isFlipped)}
                className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600 w-full"
              >
                Check Answer
              </button>
              {isFlipped && (
                <div className="mt-4 p-4 bg-gray-100 rounded">
                  <p className="font-medium">Correct answer: {flashcards[currentCardIndex].BACK}</p>
                  <div className="flex justify-between mt-4">
                    <button className="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-600">
                      I was wrong
                    </button>
                    <button className="bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600">
                      I knew it
                    </button>
                  </div>
                </div>
              )}
            </div>
            <div className="flex space-x-4">
              <button 
                onClick={() => {
                  setIsFlipped(false);
                  prevCard();
                }}
                className="bg-gray-200 px-4 py-2 rounded hover:bg-gray-300"
              >
                Previous
              </button>
              <button 
                onClick={() => {
                  setIsFlipped(false);
                  nextCard();
                }}
                className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
              >
                Next
              </button>
            </div>
            <div className="mt-4">
              <p>{currentCardIndex + 1} of {flashcards.length}</p>
            </div>
          </div>
        )}
      </div>

      {/* Create Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-lg p-6 w-full max-w-md">
            <h2 className="text-xl font-bold mb-4">
              Create New {modalType === 'folder' ? 'Folder' : 'Set'}
            </h2>
            <form onSubmit={handleCreate}>
              <div className="mb-4">
                <label className="block text-gray-700 mb-2">
                  Name
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="border rounded w-full px-3 py-2"
                  required
                />
              </div>
              {modalType === 'set' && (
                <div className="mb-4">
                  <label className="block text-gray-700 mb-2">
                    Folder
                  </label>
                  <select
                    value={formData.folder_id || ''}
                    onChange={(e) => setFormData({ ...formData, folder_id: e.target.value ? parseInt(e.target.value) : null })}
                    className="border rounded w-full px-3 py-2"
                  >
                    <option value="">Root (No Folder)</option>
                    {items.filter(item => item.TYPE === 'FOLDER').map(folder => (
                      <option key={`folder-opt-${folder.ID}`} value={folder.ID}>
                        {folder.NAME}
                      </option>
                    ))}
                  </select>
                </div>
              )}
              <div className="mb-4">
                <label className="block text-gray-700 mb-2">
                  Notification Date (optional)
                </label>
                <input
                  type="date"
                  value={formData.notification_date}
                  onChange={(e) => setFormData({ ...formData, notification_date: e.target.value })}
                  className="border rounded w-full px-3 py-2"
                />
              </div>
              <div className="mb-4">
                <label className="block text-gray-700 mb-2">
                  End Date (optional)
                </label>
                <input
                  type="date"
                  value={formData.end_date}
                  onChange={(e) => setFormData({ ...formData, end_date: e.target.value })}
                  className="border rounded w-full px-3 py-2"
                />
              </div>
              <div className="flex justify-end space-x-4">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="px-4 py-2 rounded border hover:bg-gray-100"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
                >
                  Create
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Edit Modal */}
      {showEditModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-lg p-6 w-full max-w-md">
            <h2 className="text-xl font-bold mb-4">
              Edit {modalType === 'folder' ? 'Folder' : editData.TYPE === 'FLASHCARD' ? 'Flashcard' : 'Set'}
            </h2>
            <form onSubmit={editData.TYPE === 'FLASHCARD' ? handleEditFlashcard : handleEdit}>
              {editData.TYPE === 'FLASHCARD' ? (
                <>
                  <div className="mb-4">
                    <label className="block text-gray-700 mb-2">
                      Front
                    </label>
                    <input
                      type="text"
                      value={formData.front}
                      onChange={(e) => setFormData({ ...formData, front: e.target.value })}
                      className="border rounded w-full px-3 py-2"
                      required
                    />
                  </div>
                  <div className="mb-4">
                    <label className="block text-gray-700 mb-2">
                      Back
                    </label>
                    <input
                      type="text"
                      value={formData.back}
                      onChange={(e) => setFormData({ ...formData, back: e.target.value })}
                      className="border rounded w-full px-3 py-2"
                      required
                    />
                  </div>
                </>
              ) : (
                <>
                  <div className="mb-4">
                    <label className="block text-gray-700 mb-2">
                      Name
                    </label>
                    <input
                      type="text"
                      value={formData.name}
                      onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      className="border rounded w-full px-3 py-2"
                      required
                    />
                  </div>
                  {modalType === 'set' && (
                    <div className="mb-4">
                      <label className="block text-gray-700 mb-2">
                        Folder
                      </label>
                      <select
                        value={formData.folder_id || ''}
                        onChange={(e) => setFormData({ ...formData, folder_id: e.target.value ? parseInt(e.target.value) : null })}
                        className="border rounded w-full px-3 py-2"
                      >
                        <option value="">Root (No Folder)</option>
                        {items.filter(item => item.TYPE === 'FOLDER').map(folder => (
                          <option key={`folder-opt-${folder.ID}`} value={folder.ID}>
                            {folder.NAME}
                          </option>
                        ))}
                      </select>
                    </div>
                  )}
                  <div className="mb-4">
                    <label className="block text-gray-700 mb-2">
                      Notification Date (optional)
                    </label>
                    <input
                      type="date"
                      value={formData.notification_date}
                      onChange={(e) => setFormData({ ...formData, notification_date: e.target.value })}
                      className="border rounded w-full px-3 py-2"
                    />
                  </div>
                  <div className="mb-4">
                    <label className="block text-gray-700 mb-2">
                      End Date (optional)
                    </label>
                    <input
                      type="date"
                      value={formData.end_date}
                      onChange={(e) => setFormData({ ...formData, end_date: e.target.value })}
                      className="border rounded w-full px-3 py-2"
                    />
                  </div>
                </>
              )}
              <div className="flex justify-end space-x-4">
                <button
                  type="button"
                  onClick={() => setShowEditModal(false)}
                  className="px-4 py-2 rounded border hover:bg-gray-100"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
                >
                  Save
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default QuizletClone;