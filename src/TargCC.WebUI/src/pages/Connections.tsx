/**
 * Connections Page
 * Full-page view for managing database connections
 */

import { useState } from 'react';
import {
  Box,
  Container,
  Paper,
  Typography,
  Button,
  List,
  ListItem,
  ListItemText,
  IconButton,
  CircularProgress,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import StorageIcon from '@mui/icons-material/Storage';
import { useConnections } from '../hooks/useConnections';

export default function Connections() {
  const {
    connections,
    loading,
    testConnection: testConnectionString,
    deleteConnection: removeConnection,
  } = useConnections();

  const [testing, setTesting] = useState<string | null>(null);
  const [editDialogOpen, setEditDialogOpen] = useState(false);

  const handleTest = async (connectionString: string, id: string) => {
    setTesting(id);
    try {
      const isValid = await testConnectionString(connectionString);
      alert(isValid ? 'Connection successful! ✅' : 'Connection failed ❌');
    } catch (error) {
      alert('Connection test failed ❌');
    } finally {
      setTesting(null);
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('Are you sure you want to delete this connection?')) {
      try {
        await removeConnection(id);
      } catch (error) {
        alert('Failed to delete connection');
      }
    }
  };

  const formatLastUsed = (date: string): string => {
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
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Paper sx={{ p: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
          <Typography variant="h5" component="h1">
            Database Connections
          </Typography>
          <Button
            startIcon={<AddIcon />}
            variant="contained"
            onClick={() => setEditDialogOpen(true)}
          >
            Add Connection
          </Button>
        </Box>

        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
            <CircularProgress />
          </Box>
        ) : connections.length === 0 ? (
          <Box sx={{ textAlign: 'center', p: 4 }}>
            <StorageIcon sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
            <Typography variant="h6" color="text.secondary" gutterBottom>
              No connections yet
            </Typography>
            <Typography color="text.secondary" sx={{ mb: 2 }}>
              Add your first database connection to get started
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
                  mb: 2,
                  bgcolor: 'background.paper',
                }}
              >
                <StorageIcon sx={{ mr: 2, color: 'primary.main', fontSize: 40 }} />
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="h6">{conn.name}</Typography>
                      {conn.useIntegratedSecurity && (
                        <Chip label="Integrated Security" size="small" color="primary" variant="outlined" />
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
                <Box sx={{ display: 'flex', gap: 1 }}>
                  <IconButton
                    color="success"
                    onClick={() => handleTest(conn.connectionString, conn.id)}
                    disabled={testing === conn.id}
                    title="Test Connection"
                  >
                    {testing === conn.id ? (
                      <CircularProgress size={24} />
                    ) : (
                      <CheckCircleIcon />
                    )}
                  </IconButton>
                  <IconButton
                    color="primary"
                    onClick={() => setEditDialogOpen(true)}
                    title="Edit Connection"
                  >
                    <EditIcon />
                  </IconButton>
                  <IconButton
                    color="error"
                    onClick={() => handleDelete(conn.id)}
                    title="Delete Connection"
                  >
                    <DeleteIcon />
                  </IconButton>
                </Box>
              </ListItem>
            ))}
          </List>
        )}
      </Paper>

      {/* Add/Edit Connection Dialog */}
      <Dialog open={editDialogOpen} onClose={() => setEditDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Add Connection</DialogTitle>
        <DialogContent>
          <Typography color="text.secondary" sx={{ mt: 1 }}>
            Connection form will be implemented in a future update.
            <br />
            For now, connections are managed through the backend API.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}
