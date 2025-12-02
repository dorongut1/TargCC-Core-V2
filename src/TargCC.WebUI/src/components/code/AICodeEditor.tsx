/**
 * AICodeEditor Component
 *
 * AI-powered code editor that allows users to modify generated code using natural language.
 * Integrates Monaco Editor, AI chat, diff viewing, and validation.
 *
 * Features:
 * - Natural language code modification via AI
 * - Real-time code editing with Monaco Editor
 * - Change tracking with diff viewer
 * - Code validation before applying changes
 * - Conversation history tracking
 * - Undo/Redo functionality
 * - Theme support (dark/light)
 */

import { useState, useCallback, useEffect } from 'react';
import {
  Box,
  Paper,
  Grid,
  Typography,
  Alert,
  CircularProgress,
  Tabs,
  Tab,
  IconButton,
  Tooltip,
  Chip,
} from '@mui/material';
import UndoIcon from '@mui/icons-material/Undo';
import RedoIcon from '@mui/icons-material/Redo';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import WarningIcon from '@mui/icons-material/Warning';
import ErrorIcon from '@mui/icons-material/Error';
import Editor from '@monaco-editor/react';
import { modifyCode } from '../../api/aiCodeEditorApi';
import AIChatPanel from './AIChatPanel';
import CodeDiffViewer from './CodeDiffViewer';
import type {
  CodeModificationRequest,
  CodeModificationResponse,
  ChatMessage,
  CodeChange,
  ValidationError,
  ValidationWarning,
} from '../../types/aiCodeEditor';

interface AICodeEditorProps {
  initialCode: string;
  tableName: string;
  schema?: string;
  relatedTables?: string[];
  language?: string;
  height?: string;
  onCodeChange?: (code: string) => void;
  onSave?: (code: string) => void;
}

interface HistoryEntry {
  code: string;
  changes: CodeChange[];
  explanation?: string;
  timestamp: Date;
}

const AICodeEditor = ({
  initialCode,
  tableName,
  schema = 'dbo',
  relatedTables = [],
  language = 'typescript',
  height = '600px',
  onCodeChange,
  onSave,
}: AICodeEditorProps) => {
  // State
  const [currentCode, setCurrentCode] = useState(initialCode);
  const [originalCode, setOriginalCode] = useState(initialCode);
  const [chatHistory, setChatHistory] = useState<ChatMessage[]>([]);
  const [conversationId, setConversationId] = useState<string>('');
  const [isModifying, setIsModifying] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<ValidationError[]>([]);
  const [validationWarnings, setValidationWarnings] = useState<ValidationWarning[]>([]);
  const [changes, setChanges] = useState<CodeChange[]>([]);
  const [activeTab, setActiveTab] = useState<'editor' | 'diff'>('editor');
  const [theme, setTheme] = useState<'vs-dark' | 'light'>('vs-dark');

  // History for undo/redo
  const [history, setHistory] = useState<HistoryEntry[]>([
    { code: initialCode, changes: [], timestamp: new Date() },
  ]);
  const [historyIndex, setHistoryIndex] = useState(0);

  // Handle AI instruction submission
  const handleAIInstruction = useCallback(
    async (instruction: string) => {
      setIsModifying(true);
      setError(null);

      // Add user message to chat
      const userMessage: ChatMessage = {
        role: 'user',
        content: instruction,
        timestamp: new Date(),
      };
      setChatHistory((prev) => [...prev, userMessage]);

      try {
        const request: CodeModificationRequest = {
          originalCode: currentCode,
          instruction,
          tableName,
          schema,
          relatedTables,
          conversationId: conversationId || undefined,
        };

        const response: CodeModificationResponse = await modifyCode(request);

        if (!response.success) {
          throw new Error(response.errorMessage || 'Failed to modify code');
        }

        // Update conversation ID
        if (response.conversationId) {
          setConversationId(response.conversationId);
        }

        // Update code
        setCurrentCode(response.modifiedCode);
        setChanges(response.changes);

        // Update validation
        setValidationErrors(response.validation.errors);
        setValidationWarnings(response.validation.warnings);

        // Add assistant message to chat
        const assistantMessage: ChatMessage = {
          role: 'assistant',
          content: response.explanation || 'Code modified successfully',
          timestamp: new Date(),
        };
        setChatHistory((prev) => [...prev, assistantMessage]);

        // Add to history
        const newEntry: HistoryEntry = {
          code: response.modifiedCode,
          changes: response.changes,
          explanation: response.explanation,
          timestamp: new Date(),
        };
        setHistory((prev) => [...prev.slice(0, historyIndex + 1), newEntry]);
        setHistoryIndex((prev) => prev + 1);

        // Switch to diff view to show changes
        setActiveTab('diff');

        // Notify parent
        onCodeChange?.(response.modifiedCode);
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : 'Unknown error occurred';
        setError(errorMessage);

        // Add error message to chat
        const errorChatMessage: ChatMessage = {
          role: 'assistant',
          content: `Error: ${errorMessage}`,
          timestamp: new Date(),
        };
        setChatHistory((prev) => [...prev, errorChatMessage]);
      } finally {
        setIsModifying(false);
      }
    },
    [currentCode, tableName, schema, relatedTables, conversationId, historyIndex, onCodeChange]
  );

  // Handle manual code edits
  const handleCodeEdit = useCallback(
    (newCode: string | undefined) => {
      if (newCode !== undefined) {
        setCurrentCode(newCode);
        onCodeChange?.(newCode);
      }
    },
    [onCodeChange]
  );

  // Undo
  const handleUndo = useCallback(() => {
    if (historyIndex > 0) {
      const newIndex = historyIndex - 1;
      setHistoryIndex(newIndex);
      setCurrentCode(history[newIndex].code);
      setChanges(history[newIndex].changes);
      onCodeChange?.(history[newIndex].code);
    }
  }, [historyIndex, history, onCodeChange]);

  // Redo
  const handleRedo = useCallback(() => {
    if (historyIndex < history.length - 1) {
      const newIndex = historyIndex + 1;
      setHistoryIndex(newIndex);
      setCurrentCode(history[newIndex].code);
      setChanges(history[newIndex].changes);
      onCodeChange?.(history[newIndex].code);
    }
  }, [historyIndex, history, onCodeChange]);

  // Reset to original
  const handleReset = useCallback(() => {
    setCurrentCode(originalCode);
    setChanges([]);
    setValidationErrors([]);
    setValidationWarnings([]);
    setChatHistory([]);
    setHistory([{ code: originalCode, changes: [], timestamp: new Date() }]);
    setHistoryIndex(0);
    setActiveTab('editor');
    onCodeChange?.(originalCode);
  }, [originalCode, onCodeChange]);

  const canUndo = historyIndex > 0;
  const canRedo = historyIndex < history.length - 1;
  const hasChanges = currentCode !== originalCode;
  const hasErrors = validationErrors.length > 0;
  const hasWarnings = validationWarnings.length > 0;

  return (
    <Box>
      <Paper sx={{ mb: 2, p: 2 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Box>
            <Typography variant="h6">AI Code Editor</Typography>
            <Typography variant="body2" color="text.secondary">
              {tableName} ({schema})
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
            <Tooltip title="Undo">
              <span>
                <IconButton onClick={handleUndo} disabled={!canUndo} size="small">
                  <UndoIcon />
                </IconButton>
              </span>
            </Tooltip>

            <Tooltip title="Redo">
              <span>
                <IconButton onClick={handleRedo} disabled={!canRedo} size="small">
                  <RedoIcon />
                </IconButton>
              </span>
            </Tooltip>

            {hasChanges && (
              <Chip
                label={`${changes.length} changes`}
                size="small"
                color="info"
                icon={<CheckCircleIcon />}
              />
            )}

            {hasErrors && (
              <Chip
                label={`${validationErrors.length} errors`}
                size="small"
                color="error"
                icon={<ErrorIcon />}
              />
            )}

            {hasWarnings && (
              <Chip
                label={`${validationWarnings.length} warnings`}
                size="small"
                color="warning"
                icon={<WarningIcon />}
              />
            )}
          </Box>
        </Box>

        {error && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
            {error}
          </Alert>
        )}

        <Tabs value={activeTab} onChange={(_, val) => setActiveTab(val)} sx={{ mb: 2 }}>
          <Tab label="Editor" value="editor" />
          <Tab label={`Diff (${changes.length})`} value="diff" disabled={changes.length === 0} />
        </Tabs>
      </Paper>

      <Grid container spacing={2}>
        <Grid item xs={12} md={8}>
          <Paper sx={{ p: 2, height: `calc(${height} + 100px)`, overflow: 'auto' }}>
            {activeTab === 'editor' && (
              <Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                  <Typography variant="subtitle2">Code Editor</Typography>
                </Box>
                <Editor
                  height={height}
                  language={language}
                  value={currentCode}
                  theme={theme}
                  onChange={handleCodeEdit}
                  options={{
                    readOnly: false,
                    minimap: { enabled: true },
                    scrollBeyondLastLine: false,
                    fontSize: 14,
                    lineNumbers: 'on',
                    folding: true,
                    automaticLayout: true,
                    wordWrap: 'on',
                  }}
                />
              </Box>
            )}

            {activeTab === 'diff' && (
              <CodeDiffViewer
                originalCode={originalCode}
                modifiedCode={currentCode}
                changes={changes}
                height={height}
              />
            )}
          </Paper>
        </Grid>

        <Grid item xs={12} md={4}>
          <AIChatPanel
            chatHistory={chatHistory}
            onSendMessage={handleAIInstruction}
            isLoading={isModifying}
            height={`calc(${height} + 100px)`}
          />
        </Grid>
      </Grid>
    </Box>
  );
};

export default AICodeEditor;
