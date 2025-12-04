import {
  Box,
  FormGroup,
  FormControlLabel,
  Checkbox,
  Typography,
  Alert,
  Paper
} from '@mui/material';
import type { WizardData } from './GenerationWizard';

interface GenerationOptionsProps {
  data: WizardData;
  onChange: (data: WizardData) => void;
}

const optionDescriptions = {
  entities: {
    label: 'Entity Classes',
    description: 'Generate domain model classes with properties and relationships'
  },
  repositories: {
    label: 'Repositories',
    description: 'Generate data access layer with CRUD operations'
  },
  handlers: {
    label: 'CQRS Handlers',
    description: 'Generate command and query handlers using MediatR'
  },
  api: {
    label: 'API Controllers',
    description: 'Generate REST API endpoints with standard CRUD operations'
  },
  reactUI: {
    label: 'React UI Components 🎨',
    description: 'Generate React Form, List, Detail components with Material-UI (900-1000 lines per table)'
  }
};

const GenerationOptions = ({ data, onChange }: GenerationOptionsProps) => {
  const handleOptionChange = (option: keyof typeof data.options) => {
    onChange({
      ...data,
      options: {
        ...data.options,
        [option]: !data.options[option]
      }
    });
  };

  const hasAnyOption = Object.values(data.options).some((v) => v);
  const selectedCount = Object.values(data.options).filter(Boolean).length;

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Choose What to Generate
      </Typography>

      <Typography variant="body2" color="text.secondary" paragraph>
        Select the components you want to generate for the selected tables
      </Typography>

      {!hasAnyOption && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          Please select at least one generation option
        </Alert>
      )}

      <FormGroup>
        {(Object.keys(optionDescriptions) as Array<keyof typeof data.options>).map((key) => (
          <Paper key={key} sx={{ p: 2, mb: 2 }}>
            <FormControlLabel
              control={
                <Checkbox
                  checked={data.options[key]}
                  onChange={() => handleOptionChange(key)}
                />
              }
              label={
                <Box>
                  <Typography variant="subtitle1">
                    {optionDescriptions[key].label}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {optionDescriptions[key].description}
                  </Typography>
                </Box>
              }
            />
          </Paper>
        ))}
      </FormGroup>

      <Typography variant="caption" color="text.secondary" sx={{ mt: 2, display: 'block' }}>
        {selectedCount} option(s) selected
      </Typography>
    </Box>
  );
};

export default GenerationOptions;
