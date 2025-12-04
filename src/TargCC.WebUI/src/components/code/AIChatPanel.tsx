/**
 * AIChatPanel Component
 *
 * Chat interface for AI-powered code modification.
 * Allows users to send natural language instructions to modify code.
 *
 * Features:
 * - Chat message history
 * - Message input with send button
 * - Loading state during AI processing
 * - Auto-scroll to latest message
 * - Example prompts for guidance
 */

import { useState, useRef, useEffect } from 'react';
import {
  Box,
  Paper,
  TextField,
  IconButton,
  Typography,
  List,
  ListItem,
  ListItemText,
  CircularProgress,
  Chip,
  Divider,
} from '@mui/material';
import SendIcon from '@mui/icons-material/Send';
import SmartToyIcon from '@mui/icons-material/SmartToy';
import PersonIcon from '@mui/icons-material/Person';
import type { ChatMessage } from '../../types/aiCodeEditor';

interface AIChatPanelProps {
  chatHistory: ChatMessage[];
  onSendMessage: (message: string) => void;
  isLoading: boolean;
  height?: string;
}

const EXAMPLE_PROMPTS = [
  'Make the save button blue',
  'Add email validation',
  'Change grid to 2 columns',
  'Add loading spinner',
  'Move submit button to left',
];

const AIChatPanel = ({
  chatHistory,
  onSendMessage,
  isLoading,
  height = '600px',
}: AIChatPanelProps) => {
  const [message, setMessage] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Auto-scroll to latest message
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [chatHistory]);

  const handleSend = () => {
    if (message.trim() && !isLoading) {
      onSendMessage(message.trim());
      setMessage('');
    }
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const handleExampleClick = (prompt: string) => {
    setMessage(prompt);
  };

  return (
    <Paper
      sx={{
        height,
        display: 'flex',
        flexDirection: 'column',
        overflow: 'hidden',
      }}
    >
      {/* Header */}
      <Box sx={{ p: 2, bgcolor: 'primary.main', color: 'white' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <SmartToyIcon />
          <Typography variant="h6">AI Assistant</Typography>
        </Box>
        <Typography variant="caption">
          Tell me how you'd like to modify the code
        </Typography>
      </Box>

      {/* Example Prompts */}
      {chatHistory.length === 0 && (
        <Box sx={{ p: 2, bgcolor: 'grey.50' }}>
          <Typography variant="caption" color="text.secondary" sx={{ mb: 1, display: 'block' }}>
            Try these examples:
          </Typography>
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
            {EXAMPLE_PROMPTS.map((prompt, index) => (
              <Chip
                key={index}
                label={prompt}
                size="small"
                onClick={() => handleExampleClick(prompt)}
                sx={{ cursor: 'pointer' }}
              />
            ))}
          </Box>
        </Box>
      )}

      {/* Messages */}
      <Box
        sx={{
          flex: 1,
          overflowY: 'auto',
          p: 2,
          bgcolor: 'grey.50',
        }}
      >
        {chatHistory.length === 0 ? (
          <Box
            sx={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              justifyContent: 'center',
              height: '100%',
              color: 'text.secondary',
            }}
          >
            <SmartToyIcon sx={{ fontSize: 60, mb: 2, opacity: 0.3 }} />
            <Typography variant="body2" align="center">
              No messages yet.
              <br />
              Start a conversation by describing
              <br />
              how you'd like to modify the code.
            </Typography>
          </Box>
        ) : (
          <List sx={{ p: 0 }}>
            {chatHistory.map((msg, index) => (
              <ListItem
                key={index}
                sx={{
                  flexDirection: 'column',
                  alignItems: msg.role === 'user' ? 'flex-end' : 'flex-start',
                  px: 0,
                  py: 1,
                }}
              >
                <Box
                  sx={{
                    maxWidth: '85%',
                    display: 'flex',
                    gap: 1,
                    alignItems: 'flex-start',
                    flexDirection: msg.role === 'user' ? 'row-reverse' : 'row',
                  }}
                >
                  <Box
                    sx={{
                      width: 32,
                      height: 32,
                      borderRadius: '50%',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      bgcolor: msg.role === 'user' ? 'primary.main' : 'secondary.main',
                      color: 'white',
                      flexShrink: 0,
                    }}
                  >
                    {msg.role === 'user' ? <PersonIcon fontSize="small" /> : <SmartToyIcon fontSize="small" />}
                  </Box>

                  <Paper
                    elevation={1}
                    sx={{
                      p: 1.5,
                      bgcolor: msg.role === 'user' ? 'primary.light' : 'white',
                      color: msg.role === 'user' ? 'primary.contrastText' : 'text.primary',
                    }}
                  >
                    <ListItemText
                      primary={msg.content}
                      secondary={new Date(msg.timestamp).toLocaleTimeString()}
                      secondaryTypographyProps={{
                        sx: {
                          color: msg.role === 'user' ? 'primary.contrastText' : 'text.secondary',
                          opacity: 0.7,
                          fontSize: '0.7rem',
                          mt: 0.5,
                        },
                      }}
                      primaryTypographyProps={{
                        sx: {
                          whiteSpace: 'pre-wrap',
                          wordBreak: 'break-word',
                        },
                      }}
                    />
                  </Paper>
                </Box>
              </ListItem>
            ))}

            {isLoading && (
              <ListItem sx={{ px: 0, py: 1, justifyContent: 'flex-start' }}>
                <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                  <Box
                    sx={{
                      width: 32,
                      height: 32,
                      borderRadius: '50%',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      bgcolor: 'secondary.main',
                      color: 'white',
                    }}
                  >
                    <SmartToyIcon fontSize="small" />
                  </Box>
                  <Paper elevation={1} sx={{ p: 1.5 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <CircularProgress size={16} />
                      <Typography variant="body2" color="text.secondary">
                        Analyzing and modifying code...
                      </Typography>
                    </Box>
                  </Paper>
                </Box>
              </ListItem>
            )}

            <div ref={messagesEndRef} />
          </List>
        )}
      </Box>

      <Divider />

      {/* Input */}
      <Box sx={{ p: 2, bgcolor: 'background.paper' }}>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <TextField
            fullWidth
            multiline
            maxRows={3}
            placeholder="Describe the changes you want..."
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            onKeyPress={handleKeyPress}
            disabled={isLoading}
            size="small"
          />
          <IconButton
            color="primary"
            onClick={handleSend}
            disabled={!message.trim() || isLoading}
            sx={{ alignSelf: 'flex-end' }}
          >
            <SendIcon />
          </IconButton>
        </Box>
      </Box>
    </Paper>
  );
};

export default AIChatPanel;
