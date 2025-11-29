# Next Session: Day 26 - Generation Wizard Foundation (Part 1)

**Date:** 30/11/2025  
**Phase:** 3C - Local Web UI  
**Day:** 26 of 45  
**Duration:** ~3 hours  
**Status:** Ready to Start

---

## 🎯 Day 26 Objectives

### Primary Goal
Create the foundation for a multi-step Generation Wizard that guides users through the code generation process.

### Specific Deliverables

1. **Wizard Component** (MUI Stepper)
   - Multi-step wizard with 4 steps
   - Step navigation (Next, Back, Finish)
   - Step validation
   - Progress tracking

2. **Step 1: Table Selection**
   - Table list with checkboxes
   - Search and filter
   - Select all/none
   - Validation (at least 1 table)

3. **Step 2: Generation Options**
   - Checkboxes for generation types
   - Entity classes
   - Repositories
   - CQRS Handlers
   - API Controllers
   - Validation (at least 1 option)

4. **Testing**
   - 15-20 new tests
   - Wizard navigation tests
   - Validation tests
   - Step component tests

---

## 📋 Detailed Implementation Plan

### Part 1: Wizard Component (60 minutes)

#### 1.1 GenerationWizard Component
```typescript
// GenerationWizard.tsx
import { useState } from 'react';
import {
  Stepper,
  Step,
  StepLabel,
  Box,
  Button,
  Paper
} from '@mui/material';

interface WizardStep {
  label: string;
  component: React.ComponentType<any>;
  validate?: () => boolean;
}

const steps: WizardStep[] = [
  { label: 'Select Tables', component: TableSelection },
  { label: 'Choose Options', component: GenerationOptions },
  { label: 'Review', component: ReviewStep },
  { label: 'Generate', component: GenerationProgress }
];

const GenerationWizard = () => {
  const [activeStep, setActiveStep] = useState(0);
  const [wizardData, setWizardData] = useState({
    selectedTables: [] as string[],
    options: {
      entities: true,
      repositories: true,
      handlers: true,
      api: true
    }
  });

  const handleNext = () => {
    const currentStep = steps[activeStep];
    if (currentStep.validate && !currentStep.validate()) {
      return; // Validation failed
    }
    setActiveStep((prev) => prev + 1);
  };

  const handleBack = () => {
    setActiveStep((prev) => prev - 1);
  };

  const handleFinish = async () => {
    // Trigger code generation
    await generateCode(wizardData);
  };

  const CurrentStepComponent = steps[activeStep].component;

  return (
    <Box sx={{ width: '100%', p: 3 }}>
      <Stepper activeStep={activeStep} sx={{ mb: 4 }}>
        {steps.map((step) => (
          <Step key={step.label}>
            <StepLabel>{step.label}</StepLabel>
          </Step>
        ))}
      </Stepper>

      <Paper sx={{ p: 3, minHeight: 400 }}>
        <CurrentStepComponent
          data={wizardData}
          onChange={setWizardData}
        />
      </Paper>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 3 }}>
        <Button
          disabled={activeStep === 0}
          onClick={handleBack}
        >
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
```

---

### Part 2: Table Selection Step (45 minutes)

#### 2.1 TableSelection Component
```typescript
// TableSelection.tsx
import { useState } from 'react';
import {
  Box,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Checkbox,
  TextField,
  Typography,
  Button
} from '@mui/material';
import { Table as TableIcon } from '@mui/icons-material';

interface TableSelectionProps {
  data: WizardData;
  onChange: (data: WizardData) => void;
}

const TableSelection = ({ data, onChange }: TableSelectionProps) => {
  const [searchTerm, setSearchTerm] = useState('');
  const allTables = ['Customer', 'Order', 'Product', 'Employee', 'Invoice'];

  const filteredTables = allTables.filter((table) =>
    table.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleToggle = (table: string) => {
    const newSelection = data.selectedTables.includes(table)
      ? data.selectedTables.filter((t) => t !== table)
      : [...data.selectedTables, table];
    
    onChange({ ...data, selectedTables: newSelection });
  };

  const handleSelectAll = () => {
    onChange({ ...data, selectedTables: filteredTables });
  };

  const handleSelectNone = () => {
    onChange({ ...data, selectedTables: [] });
  };

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Select Tables to Generate
      </Typography>

      <Box sx={{ mb: 2, display: 'flex', gap: 2 }}>
        <TextField
          fullWidth
          placeholder="Search tables..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
        <Button onClick={handleSelectAll}>Select All</Button>
        <Button onClick={handleSelectNone}>Select None</Button>
      </Box>

      <List sx={{ maxHeight: 300, overflow: 'auto' }}>
        {filteredTables.map((table) => (
          <ListItem key={table} disablePadding>
            <ListItemButton onClick={() => handleToggle(table)}>
              <ListItemIcon>
                <Checkbox
                  checked={data.selectedTables.includes(table)}
                  tabIndex={-1}
                  disableRipple
                />
              </ListItemIcon>
              <ListItemIcon>
                <TableIcon />
              </ListItemIcon>
              <ListItemText primary={table} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>

      <Typography variant="caption" color="text.secondary" sx={{ mt: 2 }}>
        {data.selectedTables.length} table(s) selected
      </Typography>
    </Box>
  );
};

export default TableSelection;
```

---

### Part 3: Generation Options Step (45 minutes)

#### 3.1 GenerationOptions Component
```typescript
// GenerationOptions.tsx
import {
  Box,
  FormGroup,
  FormControlLabel,
  Checkbox,
  Typography,
  Alert
} from '@mui/material';

interface GenerationOptionsProps {
  data: WizardData;
  onChange: (data: WizardData) => void;
}

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

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Choose What to Generate
      </Typography>

      {!hasAnyOption && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          Please select at least one generation option
        </Alert>
      )}

      <FormGroup>
        <FormControlLabel
          control={
            <Checkbox
              checked={data.options.entities}
              onChange={() => handleOptionChange('entities')}
            />
          }
          label="Entity Classes (Domain Models)"
        />
        
        <FormControlLabel
          control={
            <Checkbox
              checked={data.options.repositories}
              onChange={() => handleOptionChange('repositories')}
            />
          }
          label="Repositories (Data Access)"
        />
        
        <FormControlLabel
          control={
            <Checkbox
              checked={data.options.handlers}
              onChange={() => handleOptionChange('handlers')}
            />
          }
          label="CQRS Handlers (Commands & Queries)"
        />
        
        <FormControlLabel
          control={
            <Checkbox
              checked={data.options.api}
              onChange={() => handleOptionChange('api')}
            />
          }
          label="API Controllers (REST Endpoints)"
        />
      </FormGroup>

      <Typography variant="caption" color="text.secondary" sx={{ mt: 2 }}>
        {Object.values(data.options).filter(Boolean).length} option(s) selected
      </Typography>
    </Box>
  );
};

export default GenerationOptions;
```

---

## 🧪 Testing Strategy

### Total Tests Target: 15-20 new tests

#### GenerationWizard Tests (7-8 tests)
```typescript
// GenerationWizard.test.tsx
- Renders all steps in stepper
- Starts at step 0
- Next button advances step
- Back button goes to previous step
- Back button disabled on first step
- Finish button appears on last step
- Validates current step before advancing
- Updates wizard data correctly
```

#### TableSelection Tests (5-6 tests)
```typescript
// TableSelection.test.tsx
- Renders table list
- Search filters tables
- Toggle selection works
- Select all selects filtered tables
- Select none clears selection
- Shows selected count
```

#### GenerationOptions Tests (5-6 tests)
```typescript
// GenerationOptions.test.tsx
- Renders all option checkboxes
- Toggle option works
- Shows warning when no options selected
- Shows selected count
- Updates wizard data on change
```

---

## 📁 Files to Create

### New Component Files
```
src/components/wizard/
├── GenerationWizard.tsx        (180 lines)
├── TableSelection.tsx          (120 lines)
├── GenerationOptions.tsx       (100 lines)
├── ReviewStep.tsx              (80 lines) - Day 27
└── GenerationProgress.tsx      (100 lines) - Day 27
```

### New Test Files
```
src/__tests__/wizard/
├── GenerationWizard.test.tsx   (120 lines)
├── TableSelection.test.tsx     (100 lines)
└── GenerationOptions.test.tsx  (90 lines)
```

### Modified Files
```
src/App.tsx                     (Add route for /wizard)
src/pages/Dashboard.tsx         (Add "New Generation" button)
```

---

## ✅ Success Criteria

### Functionality
- [ ] Wizard renders with all 4 steps
- [ ] Step navigation works (Next/Back)
- [ ] Table selection works correctly
- [ ] Generation options work correctly
- [ ] Validation prevents advancement with invalid data
- [ ] Wizard data updates properly

### Testing
- [ ] 15-20 new tests written
- [ ] All tests have correct logic
- [ ] Tests cover happy paths and validation
- [ ] Tests cover edge cases

### Code Quality
- [ ] TypeScript strict mode compliant
- [ ] No build errors or warnings
- [ ] Components under 200 lines each
- [ ] Proper prop types and interfaces
- [ ] Clean, readable code

### Documentation
- [ ] Updated Phase3_Checklist.md
- [ ] Updated STATUS.md
- [ ] Updated HANDOFF.md for Day 27
- [ ] Code comments where needed

---

## 🚀 Getting Started

### 1. Environment Setup (2 min)
```bash
cd C:\Disk1\TargCC-Core-V2\src\TargCC.WebUI

# Verify app runs
npm run dev
# Open http://localhost:5173
```

### 2. Create Directory Structure (2 min)
```bash
# Create wizard directory
mkdir src\components\wizard
mkdir src\__tests__\wizard

# Verify structure
dir src\components
dir src\__tests__
```

### 3. Implementation Order
1. Start with GenerationWizard (basic structure)
2. Then TableSelection step
3. Then GenerationOptions step
4. Write tests after each component
5. Test navigation flow end-to-end

---

## 💡 Tips for Success

### Development Workflow
1. **Component First:** Build UI component
2. **Test Immediately:** Write tests right after
3. **Visual Check:** Always verify in browser
4. **Iterate Fast:** Small changes, frequent checks

### Common Pitfalls to Avoid
- Don't skip validation - essential for UX
- Test navigation thoroughly
- Keep wizard data immutable
- Use MUI Stepper for consistency

### Performance Considerations
- Use React.memo for step components
- Debounce search input if needed
- Keep wizard state simple
- Validate only when needed

---

## 📞 Quick Commands

```bash
# Development
npm run dev              # Start dev server
npm test                 # Run tests
npm run build            # Build for production

# Troubleshooting
Remove-Item -Recurse -Force node_modules\.vite
npm run dev              # Clear cache and restart
```

---

**Ready to Start:** ✅  
**Estimated Duration:** 3 hours  
**Expected Output:** Wizard foundation with 2 complete steps and 15-20 tests  
**Next Day:** Day 27 - Wizard Completion (Review + Progress steps)

---

**Created:** 29/11/2025  
**Status:** Ready for Day 26! 🚀
