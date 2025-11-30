import { useState } from 'react';
import {
  Stepper,
  Step,
  StepLabel,
  Box,
  Button,
  Paper,
  Typography,
  Alert
} from '@mui/material';
import TableSelection from './TableSelection';
import GenerationOptions from './GenerationOptions';

export interface WizardData {
  selectedTables: string[];
  options: {
    entities: boolean;
    repositories: boolean;
    handlers: boolean;
    api: boolean;
  };
}

interface WizardStep {
  label: string;
  component: React.ComponentType<WizardStepProps>;
  validate?: (data: WizardData) => boolean;
}

interface WizardStepProps {
  data: WizardData;
  onChange: (data: WizardData) => void;
}

const ReviewStep = ({ data }: WizardStepProps) => (
  <Box>
    <Typography variant="h6" gutterBottom>
      Review Your Selection
    </Typography>
    <Typography variant="body1" paragraph>
      <strong>Tables:</strong> {data.selectedTables.join(', ')}
    </Typography>
    <Typography variant="body1">
      <strong>Options:</strong>{' '}
      {Object.entries(data.options)
        .filter(([, value]) => value)
        .map(([key]) => key)
        .join(', ')}
    </Typography>
  </Box>
);

const GenerationProgress = () => (
  <Box>
    <Typography variant="h6" gutterBottom>
      Generating Code...
    </Typography>
    <Typography variant="body1">
      This would show real-time generation progress.
    </Typography>
  </Box>
);

const GenerationWizard = () => {
  const [activeStep, setActiveStep] = useState(0);
  const [wizardData, setWizardData] = useState<WizardData>({
    selectedTables: [],
    options: {
      entities: true,
      repositories: true,
      handlers: true,
      api: true
    }
  });
  const [validationError, setValidationError] = useState<string>('');

  const steps: WizardStep[] = [
    {
      label: 'Select Tables',
      component: TableSelection,
      validate: (data: WizardData) => {
        if (data.selectedTables.length === 0) {
          setValidationError('Please select at least one table');
          return false;
        }
        return true;
      }
    },
    {
      label: 'Choose Options',
      component: GenerationOptions,
      validate: (data: WizardData) => {
        const hasAnyOption = Object.values(data.options).some((v) => v);
        if (!hasAnyOption) {
          setValidationError('Please select at least one generation option');
          return false;
        }
        return true;
      }
    },
    {
      label: 'Review',
      component: ReviewStep
    },
    {
      label: 'Generate',
      component: GenerationProgress
    }
  ];

  const handleNext = () => {
    const currentStep = steps[activeStep];
    setValidationError('');

    if (currentStep.validate && !currentStep.validate(wizardData)) {
      return;
    }

    setActiveStep((prev) => prev + 1);
  };

  const handleBack = () => {
    setValidationError('');
    setActiveStep((prev) => prev - 1);
  };

  const handleFinish = async () => {
    // TODO: Implement actual code generation
    console.log('Generating code with:', wizardData);
  };

  const CurrentStepComponent = steps[activeStep].component;

  return (
    <Box sx={{ width: '100%', p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Code Generation Wizard
      </Typography>

      <Stepper activeStep={activeStep} sx={{ mb: 4 }}>
        {steps.map((step) => (
          <Step key={step.label}>
            <StepLabel>{step.label}</StepLabel>
          </Step>
        ))}
      </Stepper>

      {validationError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {validationError}
        </Alert>
      )}

      <Paper sx={{ p: 3, minHeight: 400 }}>
        <CurrentStepComponent data={wizardData} onChange={setWizardData} />
      </Paper>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 3 }}>
        <Button disabled={activeStep === 0} onClick={handleBack}>
          Back
        </Button>
        <Button
          variant="contained"
          onClick={activeStep === steps.length - 1 ? handleFinish : handleNext}
        >
          {activeStep === steps.length - 1 ? 'Generate' : 'Next'}
        </Button>
      </Box>
    </Box>
  );
};

export default GenerationWizard;
