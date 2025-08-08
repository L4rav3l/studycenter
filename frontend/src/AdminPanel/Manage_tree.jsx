// Completely redesigned tree relocation system with Toast UI Editor
import React, { useEffect, useRef, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import axios from "axios";
import styled from "@emotion/styled";
import {
  Box,
  Button,
  IconButton,
  Menu,
  MenuItem,
  SpeedDial,
  SpeedDialAction,
  SpeedDialIcon,
  Typography,
} from "@mui/material";
import FolderIcon from "@mui/icons-material/Folder";
import DescriptionIcon from "@mui/icons-material/Description";
import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import ArrowRightIcon from "@mui/icons-material/ArrowRight";
import CreateNewFolderIcon from "@mui/icons-material/CreateNewFolder";
import NoteAddIcon from "@mui/icons-material/NoteAdd";
import MoreVertIcon from "@mui/icons-material/MoreVert";
import { Editor } from "@toast-ui/react-editor";
import "@toast-ui/editor/dist/toastui-editor.css";

const Container = styled("div")`
  padding: 20px;
  font-family: sans-serif;
`;

const DropZone = styled("div")`
  border: 2px dashed #ccc;
  padding: 16px;
  margin-bottom: 20px;
  text-align: center;
  color: #999;
  border-radius: 8px;
`;

const isDescendant = (parent, possibleChildId) => {
  if (!parent.children) return false;
  return parent.children.some(
    (child) =>
      child.ID === possibleChildId || isDescendant(child, possibleChildId)
  );
};

const TreeItem = ({ item, onCreate, onRename, onDelete, onMove, onEdit }) => {
  const [open, setOpen] = useState(false);
  const [anchorEl, setAnchorEl] = useState(null);
  const isCategory = item.TYPE === "CATEGORY";

  const handleDragStart = (e) => {
    e.dataTransfer.setData("application/json", JSON.stringify(item));
  };

  const handleDrop = (e) => {
    e.preventDefault();
    const dropped = JSON.parse(e.dataTransfer.getData("application/json"));

    if (dropped.ID === item.ID) return;
    if (!isCategory || dropped.TYPE === "DOCUMENT") return;
    if (dropped.TYPE === "CATEGORY" && isDescendant(dropped, item.ID)) return;

    onMove(dropped, item.ID);
  };

  const handleMenuClick = (e) => {
    e.stopPropagation();
    setAnchorEl(e.currentTarget);
  };

  const handleMenuClose = () => setAnchorEl(null);

  const handleRename = () => {
    const newName = prompt("New name:", isCategory ? item.NAME : item.TITLE);
    if (newName) onRename(item, newName);
    handleMenuClose();
  };

  const handleDelete = () => {
    const confirmation = prompt(`Type to confirm: ${isCategory ? item.NAME : item.TITLE}`);
    if (confirmation === (isCategory ? item.NAME : item.TITLE)) onDelete(item);
    else alert("Incorrect confirmation");
    handleMenuClose();
  };

  const handleCreate = (type) => {
    const value = prompt(type === "category" ? "Category name:" : "Document title:");
    if (value) onCreate(type, item.ID, value);
    handleMenuClose();
  };

  return (
    <Box ml={2} draggable onDragStart={handleDragStart} onDrop={handleDrop} onDragOver={(e) => e.preventDefault()}>
      <Box display="flex" alignItems="center" onClick={() => isCategory && setOpen(!open)}>
        {isCategory ? open ? <ArrowDropDownIcon /> : <ArrowRightIcon /> : <Box width={24} />}
        {isCategory ? <FolderIcon sx={{ mr: 1 }} /> : <DescriptionIcon sx={{ mr: 1 }} />}
        <Typography onClick={() => !isCategory && onEdit(item)}>
          {isCategory ? item.NAME : item.TITLE}
        </Typography>
        <IconButton size="small" onClick={handleMenuClick}>
          <MoreVertIcon fontSize="small" />
        </IconButton>
        <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={handleMenuClose}>
          {isCategory && [
            <MenuItem key="new-cat" onClick={() => handleCreate("category")}>Create Category</MenuItem>,
            <MenuItem key="new-doc" onClick={() => handleCreate("document")}>Create Document</MenuItem>,
          ]}
          <MenuItem onClick={handleRename}>Rename</MenuItem>
          <MenuItem onClick={handleDelete}>Delete</MenuItem>
        </Menu>
      </Box>

      {open && isCategory && item.children?.map((child) => (
        <TreeItem
          key={child.ID}
          item={child}
          onCreate={onCreate}
          onRename={onRename}
          onDelete={onDelete}
          onMove={onMove}
          onEdit={onEdit}
        />
      ))}
    </Box>
  );
};

const buildTree = (items, parentId = null) => {
  return items
    .filter((i) => i.PARENT === parentId)
    .map((i) => ({ ...i, children: buildTree(items, i.ID) }));
};

const GraphsPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [treeData, setTreeData] = useState([]);
  const [selectedItem, setSelectedItem] = useState(null);
  const [isEditing, setIsEditing] = useState(false);
  const editorRef = useRef();

  const fetchData = async () => {
    const token = localStorage.getItem("token");
    if (!token) return navigate("/login");

    try {
      const verify = await axios.post(`/api/auth/verifytoken`, { Token: token });
      if (verify.data.status !== 1) {
        localStorage.removeItem("token");
        return navigate("/login");
      }

      const res = await axios.get(`/api/graphs/itemlist?Id=${id}`, {
        headers: { Authorization: `Bearer ${token}` },
      });

      setTreeData(buildTree(res.data.items));
    } catch (e) {
      alert("Error: " + (e.response?.data?.error || e.message));
    }
  };

  const loadDocument = async (item) => {
    try {
      const token = localStorage.getItem("token");
      const res = await axios.get(`/api/graphs/document/get?Id=${item.ID}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      setSelectedItem({
        ...item,
        TITLE: res.data.title,
        Content: res.data.text,
      });
      setIsEditing(false);
    } catch (e) {
      alert("Failed to load document: " + (e.response?.data?.error || e.message));
    }
  };

  const handleCreate = async (type, parentId, value) => {
    const token = localStorage.getItem("token");
    const payload = {
      Main_parent: Number(id),
      Parent: parentId ?? null,
      [type === "category" ? "Name" : "Title"]: value,
    };

    try {
      await axios.post(`/api/graphs/${type}/create`, payload, {
        headers: { Authorization: `Bearer ${token}` },
      });
      fetchData();
    } catch (e) {
      alert("Creation error: " + (e.response?.data?.error || e.message));
    }
  };

  const handleRename = async (item, name) => {
    const token = localStorage.getItem("token");
    try {
      await axios.post(`/api/graphs/${item.TYPE.toLowerCase()}/rename`, {
        Id: item.ID,
        [item.TYPE === "CATEGORY" ? "Name" : "Title"]: name,
      }, {
        headers: { Authorization: `Bearer ${token}` },
      });
      fetchData();
    } catch (e) {
      alert("Rename error: " + (e.response?.data?.error || e.message));
    }
  };

  const handleDelete = async (item) => {
    const token = localStorage.getItem("token");
    try {
      await axios.post(`/api/graphs/${item.TYPE.toLowerCase()}/delete`, { Id: item.ID }, {
        headers: { Authorization: `Bearer ${token}` },
      });
      fetchData();
    } catch (e) {
      alert("Delete error: " + (e.response?.data?.error || e.message));
    }
  };

  const handleMove = async (item, newParentId) => {
    const token = localStorage.getItem("token");
    try {
      await axios.post(`/api/graphs/${item.TYPE.toLowerCase()}/setparent`, {
        Id: item.ID,
        Parent: newParentId ?? null,
      }, {
        headers: { Authorization: `Bearer ${token}` },
      });
      fetchData();
    } catch (e) {
      alert("Move error: " + (e.response?.data?.error || e.message));
    }
  };

  const handleSaveEditor = async () => {
    if (!selectedItem) return;
    const content = editorRef.current.getInstance().getMarkdown();

    const token = localStorage.getItem("token");
    try {
      await axios.post(`/api/graphs/document/edit`, {
        Id: selectedItem.ID,
        Title: selectedItem.TITLE,
        Text: content,
      }, {
        headers: { Authorization: `Bearer ${token}` },
      });
      setIsEditing(false);
      fetchData();
    } catch (e) {
      alert("Save error: " + (e.response?.data?.error || e.message));
    }
  };

  const handleRootDrop = (e) => {
    e.preventDefault();
    const dropped = JSON.parse(e.dataTransfer.getData("application/json"));
    handleMove(dropped, null);
  };

  useEffect(() => {
    fetchData();
  }, [id]);

  return (
    <Container>
      <Typography variant="h5" gutterBottom>Tree View</Typography>

      <Box display="flex" gap={4}>
        <Box flex={1} minWidth="300px">
          {treeData.map((item) => (
            <TreeItem
              key={item.ID}
              item={item}
              onCreate={handleCreate}
              onRename={handleRename}
              onDelete={handleDelete}
              onMove={handleMove}
              onEdit={loadDocument}
            />
          ))}
        </Box>

        <Box flex={2} borderLeft="1px solid #ccc" pl={3}>
          {selectedItem ? (
            <>
              <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                <Typography variant="h6">{selectedItem.TITLE}</Typography>
                {!isEditing && (
                  <IconButton onClick={() => setIsEditing(true)}>
                    <NoteAddIcon />
                  </IconButton>
                )}
              </Box>

              {isEditing ? (
                <>
                  <Editor
                    initialValue={selectedItem?.Content || ""}
                    previewStyle="vertical"
                    height="400px"
                    initialEditType="markdown"
                    useCommandShortcut={true}
                    ref={editorRef}
                  />
                  <Box mt={2}>
                    <Button onClick={() => setIsEditing(false)}>Cancel</Button>
                    <Button variant="contained" onClick={handleSaveEditor} sx={{ ml: 1 }}>
                      Save
                    </Button>
                  </Box>
                </>
              ) : (
                <Editor
                  key={selectedItem.ID}
                  initialValue={selectedItem?.Content || ""}
                  previewStyle="vertical"
                  height="400px"
                  initialEditType="markdown"
                  toolbarItems={[]}
                  hideModeSwitch={true}
                  readOnly={true}
                />
              )}
            </>
          ) : (
            <Typography variant="body1" color="text.secondary">
              Select a document to view.
            </Typography>
          )}
        </Box>
      </Box>

      <SpeedDial
        ariaLabel="Create"
        sx={{ position: "fixed", bottom: 16, right: 16 }}
        icon={<SpeedDialIcon />}
      >
        <SpeedDialAction
          icon={<CreateNewFolderIcon />}
          tooltipTitle="Add Category"
          onClick={() => {
            const name = prompt("Category name:");
            if (name) handleCreate("category", null, name);
          }}
        />
        <SpeedDialAction
          icon={<NoteAddIcon />}
          tooltipTitle="Add Document"
          onClick={() => {
            const title = prompt("Document title:");
            if (title) handleCreate("document", null, title);
          }}
        />
      </SpeedDial>
    </Container>
  );
};

export default GraphsPage;
