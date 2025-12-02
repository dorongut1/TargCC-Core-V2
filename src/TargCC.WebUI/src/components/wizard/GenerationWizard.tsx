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
import { generate } from '../../api/generationApi';

export interface WizardData {
  selectedTables: string[];
  options: {
    entities: boolean;
    repositories: boolean;
    handlers: boolean;
    api: boolean;
    reactUI: boolean;
  };
}

interface WizardStepProps {
  data: WizardData;
  onChange: (data: WizardData) => void;
  setActiveStep?: (step: number) => void;
  onGenerate?: () => Promise<void>;
}

interface WizardStep {
  label: string;
  component: React.ComponentType<WizardStepProps>;
  validate?: (data: WizardData) => boolean;
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

const GenerationProgress = ({ data, onGenerate }: WizardStepProps) => {
  const [progress, setProgress] = useState(0);
  const [progressItems, setProgressItems] = useState<ProgressItem[]>([]);
  const [isGenerating, setIsGenerating] = useState(false);
  const [isComplete, setIsComplete] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Start generation when component mounts
    const runGeneration = async () => {
      if (isGenerating || isComplete) return;

      setIsGenerating(true);
      setError(null);

      try {
        // Build expected files list for display
        const expectedFiles: ProgressItem[] = [];

        data.selectedTables.forEach((table) => {
          if (data.options.entities) {
            expectedFiles.push({
              id: `entity-${table}`,
              name: `${table}Entity.cs`,
              type: 'entity',
              status: 'pending'
            });
          }
          if (data.options.repositories) {
            expectedFiles.push({
              id: `repo-${table}`,
              name: `${table}Repository.cs`,
              type: 'repository',
              status: 'pending'
            });
          }
          if (data.options.handlers) {
            expectedFiles.push({
              id: `handler-create-${table}`,
              name: `Create${table}Handler.cs`,
              type: 'handler',
              status: 'pending'
            });
            expectedFiles.push({
              id: `handler-query-${table}`,
              name: `Get${table}Handler.cs`,
              type: 'handler',
              status: 'pending'
            });
          }
          if (data.options.api) {
            expectedFiles.push({
              id: `api-${table}`,
              name: `${table}Controller.cs`,
              type: 'api',
              status: 'pending'
            });
          }
          if (data.options.reactUI) {
            // Add React UI files (8 files per table)
            expectedFiles.push({
              id: `react-types-${table}`,
              name: `${table}.types.ts`,
              type: 'typescript',
              status: 'pending'
            });
            expectedFiles.push({
              id: `react-api-${table}`,
              name: `${table}.api.ts`,
              type: 'typescript',
              status: 'pending'
            });
            expectedFiles.push({
              id: `react-hooks-${table}`,
              name: `use${table}.ts`,
              type: 'typescript',
              status: 'pending'
            });
            expectedFiles.push({
              id: `react-form-${table}`,
              name: `${table}Form.tsx`,
              type: 'react',
              status: 'pending'
            });
            expectedFiles.push({
              id: `react-list-${table}`,
              name: `${table}List.tsx`,
              type: 'react',
              status: 'pending'
            });
            expectedFiles.push({
              id: `react-detail-${table}`,
              name: `${table}Detail.tsx`,
              type: 'react',
              status: 'pending'
            });
            expectedFiles.push({
              id: `react-routes-${table}`,
              name: `${table}Routes.tsx`,
              type: 'react',
              status: 'pending'
            });
            expectedFiles.push({
              id: `react-index-${table}`,
              name: `index.ts`,
              type: 'typescript',
              status: 'pending'
            });
          }
        });

        setProgressItems(expectedFiles);
        setProgress(10); // Show initial progress

        // Call the actual generation API
        const result = await generate({
          tableNames: data.selectedTables,
          options: {
            generateEntity: data.options.entities,
            generateRepository: data.options.repositories,
            generateStoredProcedures: true,
            generateController: data.options.api,
            generateReactUI: data.options.reactUI,
            overwriteExisting: false,
          }
        });

        if (result.success) {
          setProgress(100);

          // Use actual generated files from API if available
          if (result.generatedFiles && result.generatedFiles.length > 0) {
            const actualFiles: ProgressItem[] = result.generatedFiles.map((filePath: string, index: number) => {
              const fileName = filePath.split(/[\\/]/).pop() || filePath;

              // Determine file type from extension
              let fileType = 'default';
              if (fileName.endsWith('.cs')) {
                if (fileName.includes('Controller')) fileType = 'api';
                else if (fileName.includes('Repository')) fileType = 'repository';
                else if (fileName.includes('Handler')) fileType = 'handler';
                else fileType = 'entity';
              } else if (fileName.endsWith('.tsx')) {
                fileType = 'react';
              } else if (fileName.endsWith('.ts')) {
                fileType = 'typescript';
              }

              return {
                id: `generated-${index}`,
                name: fileName,
                type: fileType,
                status: 'complete' as const,
                message: 'Generated'
              };
            });

            setProgressItems(actualFiles);
          } else {
            // Fallback to expected files if API doesn't return file list
            const completedFiles = expectedFiles.map(file => ({
              ...file,
              status: 'complete' as const,
              message: 'Generated'
            }));

            setProgressItems(completedFiles);
          }

          setIsComplete(true);

          // Call parent handler if provided
          if (onGenerate) {
            await onGenerate();
          }
        } else {
          // Mark as error
          const errorFiles = expectedFiles.map(file => ({
            ...file,
            status: 'error' as const,
            message: result.message || 'Failed'
          }));

          setProgressItems(errorFiles);
          setError(result.message || 'Generation failed');
        }
      } catch (err) {
        console.error('Generation error:', err);
        setError(err instanceof Error ? err.message : 'Unknown error occurred');

        // Mark all as error
        const errorFiles = progressItems.map(file => ({
          ...file,
          status: 'error' as const,
          message: 'Failed'
        }));
        setProgressItems(errorFiles);
      } finally {
        setIsGenerating(false);
      }
    };

    runGeneration();
  }, []); // Empty deps - run once on mount

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        {isComplete ? '✓ Generation Complete!' : isGenerating ? 'Generating Code...' : 'Ready to Generate'}
      </Typography>

      <Typography variant="body2" color="text.secondary" paragraph>
        {isComplete
          ? `Successfully generated ${progressItems.length} files for ${data.selectedTables.length} table${data.selectedTables.length !== 1 ? 's' : ''}`
          : isGenerating
          ? `Generating ${progressItems.length} files...`
          : 'Preparing to generate code...'
        }
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Progress Tracker */}
      <Box sx={{ mb: 3 }}>
        <ProgressTracker
          items={progressItems}
          currentProgress={progress}
          estimatedTimeRemaining={isComplete ? 0 : undefined}
          currentFile={isGenerating ? 'Generating...' : undefined}
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
      api: true,
      reactUI: false
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
