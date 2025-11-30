# Next Session: Day 27 - Generation Wizard Completion

**Date:** Next Session  
**Phase:** 3C - Local Web UI  
**Day:** 27 of 45  
**Duration:** ~2-3 hours  
**Status:** Ready to Start

---

## 🎯 Day 27 Objectives

### Primary Goal
Complete the Generation Wizard by implementing the Review and Progress steps, creating a fully functional 4-step wizard flow.

### Specific Deliverables

1. **Review Step Enhancement**
   - Polished summary display
   - Selected tables with chips
   - Selected options with checkmarks
   - Edit navigation buttons
   - Clean, professional layout

2. **Generation Progress Component**
   - Mock progress indicator
   - Status messages
   - Generation log display
   - Success/error states
   - Completion UI

3. **Testing**
   - 10-12 new tests
   - Review step tests (6-8)
   - Progress step tests (4-6)

---

## 📋 Detailed Implementation Plan

### Part 1: Review Step Enhancement (45 min)

#### 1.1 Update ReviewStep in GenerationWizard.tsx
```typescript
const ReviewStep = ({ data }: WizardStepProps) => (
  <Box>
    <Typography variant="h6" gutterBottom>
      Review Your Selections
    </Typography>
    
    <Typography variant="body2" color="text.secondary" paragraph>
      Review your choices before starting code generation
    </Typography>

    {/* Selected Tables Section */}
    <Paper sx={{ p: 3, mb: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="subtitle1" fontWeight="bold">
          Selected Tables ({data.selectedTables.length})
        </Typography>
        <Button size="small" onClick={() => setActiveStep(0)}>
          Edit
        </Button>
      </Box>
      <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
        {data.selectedTables.map((table) => (
          <Chip 
            key={table} 
            label={table} 
            color="primary" 
            variant="outlined"
          />
        ))}
      </Box>
    </Paper>

    {/* Generation Options Section */}
    <Paper sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="subtitle1" fontWeight="bold">
          Generation Options
        </Typography>
        <Button size="small" onClick={() => setActiveStep(1)}>
          Edit
        </Button>
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
      types for {data.selectedTables.length} table(s)
    </Alert>
  </Box>
);
```

---

### Part 2: Generation Progress Component (60 min)

#### 2.1 Update GenerationProgress in GenerationWizard.tsx
```typescript
const GenerationProgress = ({ data }: WizardStepProps) => {
  const [progress, setProgress] = useState(0);
  const [currentStep, setCurrentStep] = useState('Initializing...');
  const [logs, setLogs] = useState<string[]>([]);
  const [isComplete, setIsComplete] = useState(false);

  useEffect(() => {
    // Simulate generation process
    const steps = [
      { progress: 10, message: 'Analyzing schema...', log: 'Reading table definitions' },
      { progress: 25, message: 'Generating entities...', log: 'Created entity classes' },
      { progress: 50, message: 'Generating repositories...', log: 'Created repository interfaces' },
      { progress: 75, message: 'Generating CQRS handlers...', log: 'Created command/query handlers' },
      { progress: 90, message: 'Generating API controllers...', log: 'Created REST endpoints' },
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
  }, []);

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        {isComplete ? '✓ Generation Complete!' : 'Generating Code...'}
      </Typography>

      <Typography variant="body2" color="text.secondary" paragraph>
        {isComplete 
          ? `Successfully generated code for ${data.selectedTables.length} table(s)`
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
      <Paper sx={{ p: 2, maxHeight: 300, overflow: 'auto', bgcolor: 'grey.50' }}>
        <Typography variant="caption" fontWeight="bold" gutterBottom display="block">
          Generation Log:
        </Typography>
        {logs.map((log, index) => (
          <Typography 
            key={index} 
            variant="caption" 
            component="div" 
            sx={{ fontFamily: 'monospace', color: 'text.secondary' }}
          >
            {log}
          </Typography>
        ))}
      </Paper>

      {/* Success State */}
      {isComplete && (
        <Alert severity="success" sx={{ mt: 3 }}>
          Code generation completed successfully! Files are ready for review.
        </Alert>
      )}
    </Box>
  );
};
```

---

### Part 3: Testing (45 min)

#### 3.1 ReviewStep Tests
```typescript
// src/__tests__/wizard/ReviewStep.test.tsx (integrated in GenerationWizard.test.tsx)

describe('GenerationWizard - Review Step', () => {
  it('displays selected tables count', () => {
    // Navigate to review step and check count
  });

  it('shows selected tables as chips', () => {
    // Verify chips are rendered
  });

  it('displays selected options with checkmarks', () => {
    // Verify options display correctly
  });

  it('Edit button navigates back to table selection', () => {
    // Test Edit button for tables
  });

  it('Edit button navigates back to options', () => {
    // Test Edit button for options
  });

  it('shows summary alert with counts', () => {
    // Verify summary information
  });
});
```

#### 3.2 Progress Step Tests
```typescript
describe('GenerationWizard - Progress Step', () => {
  it('shows progress bar', () => {
    // Verify progress bar exists
  });

  it('displays current status message', () => {
    // Check status text updates
  });

  it('shows generation log', () => {
    // Verify log entries appear
  });

  it('shows completion state when done', () => {
    // Wait for completion, check success state
  });
});
```

---

## 🧪 Testing Strategy

### Total Tests Target: 10-12 new tests

All tests will be integrated into the existing test files:
- GenerationWizard.test.tsx (6-8 new tests for Review + Progress)
- Or create separate ReviewStep.test.tsx and Progress.test.tsx if needed

---

## 📁 Files to Modify

### Update Existing
```
src/components/wizard/
└── GenerationWizard.tsx  (Enhance ReviewStep and GenerationProgress inline components)
```

### Update Tests
```
src/__tests__/wizard/
└── GenerationWizard.test.tsx  (Add 10-12 new tests)
```

Or create new test files:
```
src/__tests__/wizard/
├── ReviewStep.test.tsx         (6-8 tests)
└── GenerationProgress.test.tsx (4-6 tests)
```

---

## ✅ Success Criteria

### Functionality
- [ ] Review step shows tables and options clearly
- [ ] Edit buttons navigate to correct steps
- [ ] Progress step animates smoothly
- [ ] Log shows generation steps
- [ ] Completion state displays
- [ ] Full wizard flow works end-to-end

### Testing
- [ ] 10-12 new tests written
- [ ] All tests have correct logic
- [ ] Tests cover Review and Progress
- [ ] Edge cases tested

### Code Quality
- [ ] TypeScript strict mode compliant
- [ ] No build errors or warnings
- [ ] Clean, readable code
- [ ] Proper prop types

### Documentation
- [ ] Updated Phase3_Checklist.md
- [ ] Updated STATUS.md
- [ ] Updated HANDOFF.md for Day 28

---

## 🚀 Getting Started

### 1. Verify Environment (2 min)
```bash
cd C:\Disk1\TargCC-Core-V2\src\TargCC.WebUI

# Check if dev server is running
# If not, start it:
npm run dev

# Open browser
# http://localhost:5174/generate
```

### 2. Implementation Order
1. Enhance ReviewStep component (inline in GenerationWizard.tsx)
2. Enhance GenerationProgress component (inline in GenerationWizard.tsx)
3. Add imports (CheckCircleIcon, LinearProgress, Alert)
4. Test in browser - complete full wizard flow
5. Write tests for Review step
6. Write tests for Progress step
7. Run all tests
8. Update documentation

---

## 💡 Tips for Success

### Development Workflow
1. **Component First:** Update ReviewStep and GenerationProgress
2. **Visual Check:** Test full wizard flow in browser
3. **Test After:** Write tests once UI is working
4. **Iterate:** Small changes, frequent checks

### Common Pitfalls to Avoid
- Don't make components too complex
- Keep mock data simple
- Test the full flow, not just individual steps
- Remember: This is UI only, no real code generation yet

### Performance Considerations
- Use React.memo if needed
- Mock timers should be realistic (not too fast/slow)
- Keep log entries limited (max 10-15)

---

## 📞 Quick Commands

```bash
# Development
npm run dev              # Start dev server (if not running)
npm test                 # Run tests
npm test -- wizard       # Run wizard tests only

# Navigate to wizard
# Browser: http://localhost:5174/generate
# Or click "Generate All" from Dashboard
```

---

**Ready to Start:** ✅  
**Estimated Duration:** 2-3 hours  
**Expected Output:** Complete 4-step wizard with 10-12 tests  
**Next Day:** Day 28 - Monaco Editor Integration

---

**Created:** 30/11/2025  
**Status:** Ready for Day 27! 🚀
