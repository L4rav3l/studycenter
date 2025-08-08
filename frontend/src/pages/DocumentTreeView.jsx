import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { Link, useNavigate, useLocation } from 'react-router-dom';

export default function DocumentTreeView({ mainCategoryId }) {
  const [tree, setTree] = useState([]);
  const navigate = useNavigate();
  const token = localStorage.getItem('token');

  useEffect(() => {
    axios.get(`${process.env.REACT_APP_API_URL}/api/graphs/category/tree`, {
      params: { main_parent: mainCategoryId },
      headers: { Authorization: `Bearer ${token}` },
    }).then(res => setTree(res.data.tree))
      .catch(() => setTree([]));
  }, [mainCategoryId]);

  const renderTree = (items) => {
    return items.map(item => (
      <div key={item.id} className="ml-4">
        <div className="flex justify-between items-center">
          <span onClick={() => navigate(`document/${item.document_id}`)} className="cursor-pointer text-blue-600 hover:underline">
            📄 {item.title}
          </span>
        </div>
        {item.children && item.children.length > 0 && (
          <div className="ml-4">
            {renderTree(item.children)}
          </div>
        )}
      </div>
    ));
  };

  return (
    <div>
      <h2 className="text-xl font-bold mb-4">Dokumentumok</h2>
      {renderTree(tree)}
    </div>
  );
}