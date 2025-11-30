import { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  List,
  ListItem,
  ListItemText,
  IconButton,
  Button,
  Box,
  Typography,
  Chip,
  CircularProgress,
  TextField,
  DialogActions,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import StorageIcon from '@mui/icons-material/Storage';
import { useConnections } from '../../hooks/useConnections';
import type { Connection } from '../../api/connectionApi';

interface ConnectionManagerProps {
  open?: boolean;
  onClose?: () => void;
  onSelect?: (connection: Connection) => void;
  standalone?: boolean; // New prop for standalone page mode
}

export default function ConnectionManager({ 
  open = true, 
  onClose, 
  onSelect,
  standalone = false 
}: ConnectionManagerProps) {
  const {
    connections,
    loading,
    testConnectionString,
    removeConnection,
  } = useConnections();

  const [testing, setTesting] = useState<string | null>(null);
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [editingConnection, setEditingConnection] = useState<Connection | null>(null);

  const handleTest = async (connection: Connection) => {
    setTesting(connection.id);
    try {
      const isValid = await testConnectionString(connection.connectionString);
      alert(isValid ? 'Connection successful!' : 'Connection failed');
    } catch (error) {
      alert('Connection test failed');
    } finally {
      setTesting(null);
    }
  };

  const handleDelete = async (id: string) => {
    if (confirm('Are you sure you want to delete this connection?')) {
      try {
        await removeConnection(id);
      } catch (error) {
        alert('Failed to delete connection');
      }
    }
  };

  const handleEdit = (connection: Connection) => {
    setEditingConnection(connection);
    setEditDialogOpen(true);
  };

  const handleSelect = (connection: Connection) => {
    if (onSelect) {
      onSelect(connection);
    }
    onClose();
  };

  const formatLastUsed = (date: Date): string => {
    const d = new Date(date);
    const now = new Date();
    const diffMs = now.getTime() - d.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    
    if (diffMins < 60) return `${diffMins} minutes ago`;
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} hours ago`;
    const diffDays = Math.floor(diffHours / 24);
    return `${diffDays} days ago`;
  };

  return (
    <>
      <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
            <Typography variant="h6">Database Connections</Typography>
            <Button
              startIcon={<AddIcon />}
              variant="outlined"
              size="small"
              onClick={() => {
                setEditingConnection(null);
                setEditDialogOpen(true);
              }}
            >
              Add Connection
            </Button>
          </Box>
        </DialogTitle>
        <DialogContent>
          {loading && connections.length === 0 ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
              <CircularProgress />
            </Box>
          ) : connections.length === 0 ? (
            <Box sx={{ textAlign: 'center', p: 4 }}>
              <Typography color="text.secondary">
                No connections yet. Add your first database connection.
              </Typography>
            </Box>
          ) : (
            <List>
              {connections.map((conn) => (
                <ListItem
                  key={conn.id}
                  sx={{
                    border: 1,
                    borderColor: 'divider',
                    borderRadius: 1,
                    mb: 1,
                    cursor: onSelect ? 'pointer' : 'default',
                    '&:hover': onSelect ? { bgcolor: 'action.hover' } : {},
                  }}
                  onClick={() => onSelect && handleSelect(conn)}
                >
                  <StorageIcon sx={{ mr: 2, color: 'primary.main' }} />
                  <ListItemText
                    primary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography variant="h6">{conn.name}</Typography>
                        {conn.useIntegratedSecurity && (
                          <Chip label="Integrated" size="small" color="primary" variant="outlined" />
                        )}
                      </Box>
                    }
                    secondary={
                      <>
                        <Typography variant="body2" color="text.secondary">
                          {conn.server} / {conn.database}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          Last used: {formatLastUsed(conn.lastUsed)}
                        </Typography>
                      </>
                    }
                  />
                  <Box sx={{ display: 'flex', gap: 0.5 }}>
                    <IconButton
                      size="small"
                      onClick={(e) => {
                        e.stopPropagation();
                        handleTest(conn);
                      }}
                      disabled={testing === conn.id}
                    >
                      {testing === conn.id ? (
                        <CircularProgress size={20} />
                      ) : (
                        <CheckCircleIcon />
                      )}
                    </IconButton>
                    <IconButton
                      size="small"
                      onClick={(e) => {
                        e.stopPropagation();
                        handleEdit(conn);
                      }}
                    >
                      <EditIcon />
                    </IconButton>
                    <IconButton
                      size="small"
                      onClick={(e) => {
                        e.stopPropagation();
                        handleDelete(conn.id);
                      }}
                    >
                      <DeleteIcon />
                    </IconButton>
                  </Box>
                </ListItem>
              ))}
            </List>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Connection Edit Dialog - Simplified for now */}
      <Dialog open={editDialogOpen} onClose={() => setEditDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          {editingConnection ? 'Edit Connection' : 'Add Connection'}
        </DialogTitle>
        <DialogContent>
          <Typography color="text.secondary" sx={{ mt: 1 }}>
            Connection editing form will be implemented in a future update.
            For now, use the connection string in appsettings.json.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialogOpen(false)}>Cancel</Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
