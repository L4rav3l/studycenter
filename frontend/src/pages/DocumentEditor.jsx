import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import axios from 'axios';
import '@toast-ui/editor/dist/toastui-editor.css';
import { Editor } from '@toast-ui/react-editor';

export default function DocumentEditor() {
  const { docId } = useParams();
  const [title, setTitle] = useState('');
  const [initialValue, setInitialValue] = useState('');
  const editorRef = React.createRef();
  const token = localStorage.getItem('token');

  useEffect(() => {
    axios.get(`${process.env.REACT_APP_API_URL}/api/graphs/document/get`, {
      params: { id: docId },
      headers: { Authorization: `Bearer ${token}` },
    }).then(res => {
      setTitle(res.data.title);
      setInitialValue(res.data.text);
    });
  }, [docId]);

  const handleSave = () => {
    axios.post(`${process.env.REACT_APP_API_URL}/api/graphs/document/edit`, {
      Id: Number(docId),
      Title: title,
      Text: editorRef.current.getInstance().getMarkdown()
    }, {
      headers: { Authorization: `Bearer ${token}` }
    });
  };

  return (
    <div>
      <input
        className="w-full p-2 text-2xl font-bold border-b mb-4"
        value={title}
        onChange={e => setTitle(e.target.value)}
      />
      <Editor
        initialValue={initialValue}
        previewStyle="vertical"
        height="600px"
        initialEditType="markdown"
        useCommandShortcut={true}
        ref={editorRef}
      />
      <button onClick={handleSave} className="mt-4 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600">
        Mentés
      </button>
    </div>
  );
}
