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
  Chip
} from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import TableSelection from './TableSelection';
import GenerationOptions from './GenerationOptions';
import CodeViewer from '../code/CodeViewer';
import ProgressTracker from './ProgressTracker';
import type { ProgressItem } from './ProgressTracker';
import { mockCodeFiles } from '../../utils/mockCode';

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
  const [currentFile, setCurrentFile] = useState<string>('');
  const [progressItems, setProgressItems] = useState<ProgressItem[]>([]);
  const [estimatedTime, setEstimatedTime] = useState(30);
  const [isComplete, setIsComplete] = useState(false);

  useEffect(() => {
    // Build initial progress items based on selected options
    const initialItems: ProgressItem[] = [];
    
    data.selectedTables.forEach((table) => {
      if (data.options.entities) {
        initialItems.push({
          id: `entity-${table}`,
          name: `${table}Entity.cs`,
          type: 'entity',
          status: 'pending'
        });
      }
      if (data.options.repositories) {
        initialItems.push({
          id: `repo-${table}`,
          name: `${table}Repository.cs`,
          type: 'repository',
          status: 'pending'
        });
      }
      if (data.options.handlers) {
        initialItems.push({
          id: `handler-create-${table}`,
          name: `Create${table}Handler.cs`,
          type: 'handler',
          status: 'pending'
        });
        initialItems.push({
          id: `handler-query-${table}`,
          name: `Get${table}Handler.cs`,
          type: 'handler',
          status: 'pending'
        });
      }
      if (data.options.api) {
        initialItems.push({
          id: `api-${table}`,
          name: `${table}Controller.cs`,
          type: 'api',
          status: 'pending'
        });
      }
    });

    setProgressItems(initialItems);

    // Simulate generation process - sequential processing
    const processNextItem = (index: number) => {
      if (index >= initialItems.length) {
        setIsComplete(true);
        setCurrentFile('');
        return;
      }

      const item = initialItems[index];
      
      // Set current file and status to processing
      setCurrentFile(item.name);
      setProgressItems(prev => prev.map((p, idx) => 
        idx === index 
          ? { ...p, status: 'processing' as const, message: 'Generating...' }
          : p
      ));

      // After 600ms, mark as complete and move to next
      setTimeout(() => {
        setProgressItems(prev => prev.map((p, idx) => 
          idx === index 
            ? { ...p, status: 'complete' as const, message: 'Generated' }
            : p
        ));
        
        const newProgress = Math.round(((index + 1) / initialItems.length) * 100);
        setProgress(newProgress);
        
        // Update estimated time
        const remaining = initialItems.length - (index + 1);
        setEstimatedTime(Math.round(remaining * 0.8));
        
        // Process next item after a small delay
        setTimeout(() => processNextItem(index + 1), 200);
      }, 600);
    };

    // Start processing after a small delay
    const startTimer = setTimeout(() => processNextItem(0), 500);

    return () => {
      clearTimeout(startTimer);
    };
  }, [data.selectedTables, data.options]);

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        {isComplete ? '✓ Generation Complete!' : 'Generating Code...'}
      </Typography>

      <Typography variant="body2" color="text.secondary" paragraph>
        {isComplete 
          ? `Successfully generated ${progressItems.length} files for ${data.selectedTables.length} table${data.selectedTables.length !== 1 ? 's' : ''}`
          : `Generating ${progressItems.length} files...`
        }
      </Typography>

      {/* Progress Tracker */}
      <Box sx={{ mb: 3 }}>
        <ProgressTracker
          items={progressItems}
          currentProgress={progress}
          estimatedTimeRemaining={isComplete ? 0 : Math.round(estimatedTime)}
          currentFile={currentFile}
        />
      </Box>

      {/* Success State */}
      {isComplete && (
        <>
          <Alert severity="success" sx={{ mt: 3 }}>
            <Typography variant="body2" fontWeight="bold">
              Code generation completed successfully!
            </Typography>
            <Typography variant="caption">
              All {progressItems.length} files have been generated and are ready for review.
            </Typography>
          </Alert>

          {/* Code Preview */}
          <Box sx={{ mt: 4 }}>
            <Typography variant="h6" gutterBottom>
              Generated Code Preview
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              Preview the generated code for {data.selectedTables[0] || 'your table'}
            </Typography>
            <CodeViewer files={mockCodeFiles(data.selectedTables[0] || 'Customer')} />
          </Box>
        </>
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

      {/* Navigation buttons */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 3 }}>
        {/* Back button - always show except on first step */}
        <Button 
          disabled={activeStep === 0} 
          onClick={handleBack}
        >
          Back
        </Button>

        {/* Next button - hide on generation step (last step) */}
        {activeStep < steps.length - 1 && (
          <Button
            variant="contained"
            onClick={handleNext}
          >
            Next
          </Button>
        )}
      </Box>
    </Box>
  );
};

export default GenerationWizard;
