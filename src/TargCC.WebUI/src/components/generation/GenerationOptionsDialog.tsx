import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  FormGroup,
  FormControlLabel,
  Checkbox,
  Typography,
  Box,
  Divider,
  Alert,
} from '@mui/material';
import type { GenerationOptions } from '../../types/models';

const STORAGE_KEY = 'targcc_generation_options';

const DEFAULT_OPTIONS: GenerationOptions = {
  generateEntity: true,
  generateRepository: true,
  generateService: true,
  generateController: true,
  generateTests: true,
  overwriteExisting: false,
};

export interface GenerationOptionsDialogProps {
  open: boolean;
  onClose: () => void;
  onGenerate: (options: GenerationOptions) => void;
  tableNames: string[];
  isBulk?: boolean;
}

export default function GenerationOptionsDialog({
  open,
  onClose,
  onGenerate,
  tableNames,
  isBulk = false,
}: GenerationOptionsDialogProps) {
  const [options, setOptions] = useState<GenerationOptions>(DEFAULT_OPTIONS);

  // Load saved options from localStorage
  useEffect(() => {
    const savedOptions = localStorage.getItem(STORAGE_KEY);
    if (savedOptions) {
      try {
        const parsed = JSON.parse(savedOptions);
        setOptions({ ...DEFAULT_OPTIONS, ...parsed });
      } catch (error) {
        console.error('Failed to parse saved options:', error);
      }
    }
  }, []);

  // Save options to localStorage whenever they change
  useEffect(() => {
    if (open) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(options));
    }
  }, [options, open]);

  const handleChange = (field: keyof GenerationOptions) => (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    setOptions((prev) => ({
      ...prev,
      [field]: event.target.checked,
    }));
  };

  const handleGenerate = () => {
    onGenerate(options);
    onClose();
  };

  const handleSelectAll = () => {
    setOptions((prev) => ({
      ...prev,
      generateEntity: true,
      generateRepository: true,
      generateService: true,
      generateController: true,
      generateTests: true,
    }));
  };

  const handleDeselectAll = () => {
    setOptions((prev) => ({
      ...prev,
      generateEntity: false,
      generateRepository: false,
      generateService: false,
      generateController: false,
      generateTests: false,
    }));
  };

  const hasSelection =
    options.generateEntity ||
    options.generateRepository ||
    options.generateService ||
    options.generateController ||
    options.generateTests;

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        {isBulk
          ? `Generate Code for ${tableNames.length} Tables`
          : `Generate Code for ${tableNames[0]}`}
      </DialogTitle>
      <DialogContent>
        <Box mb={2}>
          <Typography variant="body2" color="text.secondary" gutterBottom>
            Select what to generate:
          </Typography>
        </Box>

        <FormGroup>
          <FormControlLabel
            control={
              <Checkbox
                checked={options.generateEntity}
                onChange={handleChange('generateEntity')}
              />
            }
            label="Entity Class"
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={options.generateRepository}
                onChange={handleChange('generateRepository')}
              />
            }
            label="Repository Interface & Implementation"
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={options.generateService}
                onChange={handleChange('generateService')}
              />
            }
            label="Service Interface & Implementation"
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={options.generateController}
                onChange={handleChange('generateController')}
              />
            }
            label="API Controller"
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={options.generateTests}
                onChange={handleChange('generateTests')}
              />
            }
            label="Unit Tests"
          />
        </FormGroup>

        <Box mt={2} mb={2}>
          <Divider />
        </Box>

        <FormGroup>
          <FormControlLabel
            control={
              <Checkbox
                checked={options.overwriteExisting}
                onChange={handleChange('overwriteExisting')}
                color="warning"
              />
            }
            label="Overwrite Existing Files"
          />
        </FormGroup>

        {options.overwriteExisting && (
          <Alert severity="warning" sx={{ mt: 2 }}>
            Warning: Existing files will be overwritten. This action cannot be undone.
          </Alert>
        )}

        {!hasSelection && (
          <Alert severity="error" sx={{ mt: 2 }}>
            Please select at least one item to generate.
          </Alert>
        )}

        {isBulk && (
          <Alert severity="info" sx={{ mt: 2 }}>
            Code will be generated for {tableNames.length} tables. This may take a few moments.
          </Alert>
        )}
      </DialogContent>
      <DialogActions>
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            width: '100%',
            px: 2,
          }}
        >
          <Box>
            <Button onClick={handleSelectAll} size="small">
              Select All
            </Button>
            <Button onClick={handleDeselectAll} size="small">
              Deselect All
            </Button>
          </Box>
          <Box>
            <Button onClick={onClose}>Cancel</Button>
            <Button
              onClick={handleGenerate}
              variant="contained"
              disabled={!hasSelection}
            >
              Generate
            </Button>
          </Box>
        </Box>
      </DialogActions>
    </Dialog>
  );
}
