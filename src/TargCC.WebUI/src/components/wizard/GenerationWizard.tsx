import { useState, useEffect } from 'react';
import {
  Stepper,
  Step,
  StepLabel,
  Box,
  Button,
  Paper,
  Typography,
  Alert,
  Chip,
  LinearProgress
} from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
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
  setActiveStep?: (step: number) => void;
}

const ReviewStep = ({ data, setActiveStep }: WizardStepProps) => (
  <Box>
    <Typography variant="h6" gutterBottom>
      Review Your Selections
    </Typography>
    
    <Typography variant="body2" color="text.secondary" paragraph>
      Review your choices before starting code generation
    </Typography>

    {/* Selected Tables Section */}
    <Paper sx={{ p: 3, mb: 3 }} elevation={2}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="subtitle1" fontWeight="bold">
          Selected Tables ({data.selectedTables.length})
        </Typography>
        {setActiveStep && (
          <Button size="small" onClick={() => setActiveStep(0)}>
            Edit
          </Button>
        )}
      </Box>
      <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
        {data.selectedTables.length > 0 ? (
          data.selectedTables.map((table) => (
            <Chip 
              key={table} 
              label={table} 
              color="primary" 
              variant="outlined"
            />
          ))
        ) : (
          <Typography variant="body2" color="text.secondary">
            No tables selected
          </Typography>
        )}
      </Box>
    </Paper>

    {/* Generation Options Section */}
    <Paper sx={{ p: 3 }} elevation={2}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="subtitle1" fontWeight="bold">
          Generation Options
        </Typography>
        {setActiveStep && (
          <Button size="small" onClick={() => setActiveStep(1)}>
            Edit
          </Button>
        )}
      </Box>
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
        {Object.entries(data.options)
          .filter(([, value]) => value)
          .map(([key]) => (
            <Box key={key} sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <CheckCircleIcon color="success" fontSize="small" />
              <Typography variant="body2">
                {key.charAt(0).toUpperCase() + key.slice(1)}
              </Typography>
            </Box>
          ))}
      </Box>
    </Paper>

    {/* Summary Stats */}
    <Alert severity="info" sx={{ mt: 3 }}>
      Ready to generate {Object.values(data.options).filter(Boolean).length} component 
      type{Object.values(data.options).filter(Boolean).length !== 1 ? 's' : ''} for {data.selectedTables.length} table{data.selectedTables.length !== 1 ? 's' : ''}
    </Alert>
  </Box>
);

const GenerationProgress = ({ data }: WizardStepProps) => {
  const [progress, setProgress] = useState(0);
  const [currentStep, setCurrentStep] = useState('Initializing...');
  const [logs, setLogs] = useState<string[]>([]);
  const [isComplete, setIsComplete] = useState(false);

  useEffect(() => {
    // Simulate generation process
    const steps = [
      { progress: 10, message: 'Analyzing schema...', log: 'Reading table definitions' },
      { progress: 25, message: 'Generating entities...', log: `Created ${data.selectedTables.length} entity classes` },
      { progress: 50, message: 'Generating repositories...', log: 'Created repository interfaces and implementations' },
      { progress: 75, message: 'Generating CQRS handlers...', log: 'Created command and query handlers' },
      { progress: 90, message: 'Generating API controllers...', log: 'Created REST API endpoints' },
      { progress: 100, message: 'Generation complete!', log: 'All files generated successfully' }
    ];

    let currentIndex = 0;
    const timer = setInterval(() => {
      if (currentIndex < steps.length) {
        const step = steps[currentIndex];
        setProgress(step.progress);
        setCurrentStep(step.message);
        setLogs(prev => [...prev, `[${new Date().toLocaleTimeString()}] ${step.log}`]);
        
        if (step.progress === 100) {
          setIsComplete(true);
          clearInterval(timer);
        }
        
        currentIndex++;
      }
    }, 800);

    return () => clearInterval(timer);
  }, [data.selectedTables.length]);

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        {isComplete ? '✓ Generation Complete!' : 'Generating Code...'}
      </Typography>

      <Typography variant="body2" color="text.secondary" paragraph>
        {isComplete 
          ? `Successfully generated code for ${data.selectedTables.length} table${data.selectedTables.length !== 1 ? 's' : ''}`
          : currentStep
        }
      </Typography>

      {/* Progress Bar */}
      <Box sx={{ mb: 3 }}>
        <LinearProgress 
          variant="determinate" 
          value={progress} 
          sx={{ height: 8, borderRadius: 4 }}
        />
        <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
          {progress}% Complete
        </Typography>
      </Box>

      {/* Generation Log */}
      <Paper sx={{ p: 2, maxHeight: 300, overflow: 'auto', bgcolor: 'grey.50' }} elevation={1}>
        <Typography variant="caption" fontWeight="bold" gutterBottom display="block" color="text.secondary">
          Generation Log:
        </Typography>
        {logs.length === 0 ? (
          <Typography variant="caption" color="text.secondary" sx={{ fontStyle: 'italic' }}>
            Waiting for generation to start...
          </Typography>
        ) : (
          logs.map((log, index) => (
            <Typography 
              key={index} 
              variant="caption" 
              component="div" 
              sx={{ fontFamily: 'monospace', color: 'text.secondary', py: 0.25 }}
            >
              {log}
            </Typography>
          ))
        )}
      </Paper>

      {/* Success State */}
      {isComplete && (
        <Alert severity="success" sx={{ mt: 3 }}>
          <Typography variant="body2" fontWeight="bold">
            Code generation completed successfully!
          </Typography>
          <Typography variant="caption">
            Files are ready for review and can be found in your output directory.
          </Typography>
        </Alert>
      )}
    </Box>
  );
};

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
        <CurrentStepComponent 
          data={wizardData} 
          onChange={setWizardData} 
          setActiveStep={setActiveStep}
        />
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
