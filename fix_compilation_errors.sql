-- ============================================
-- Fix Compilation Errors in TARG CC
-- ============================================
-- Issue: btnEdit control not generated because CanEdit = 'N'
-- Issue: Form not generated for RiskRuleLog because CreateUIEntity = 0
--
-- Date: 2025-12-09
-- Tables: ActiveRestrictions, RiskRuleLog
-- ============================================

USE [Upay]
GO

-- Show current settings
SELECT
    Name,
    CreateUIEntity,
    CanEdit,
    CanAdd,
    CanDelete
FROM c_Table
WHERE Name IN ('ActiveRestrictions', 'RiskRuleLog')

PRINT ''
PRINT '=== Updating c_Table settings ==='
PRINT ''

-- Fix ActiveRestrictions
UPDATE c_Table
SET
    CanEdit = 'Y'          -- Enable Edit button generation (was 'N')
WHERE Name = 'ActiveRestrictions'

PRINT '✓ ActiveRestrictions: CanEdit set to Y'

-- Fix RiskRuleLog
UPDATE c_Table
SET
    CreateUIEntity = 1,    -- Generate form/controls (was 0)
    CanEdit = 'Y'          -- Enable Edit button generation (was 'N')
WHERE Name = 'RiskRuleLog'

PRINT '✓ RiskRuleLog: CreateUIEntity set to 1, CanEdit set to Y'

PRINT ''
PRINT '=== Verification: New settings ==='
PRINT ''

-- Verify changes
SELECT
    Name,
    CreateUIEntity,
    CanEdit,
    CanAdd,
    CanDelete
FROM c_Table
WHERE Name IN ('ActiveRestrictions', 'RiskRuleLog')

PRINT ''
PRINT '=== Done! ==='
PRINT 'You can now run TARG CC to regenerate the VB.NET code'
PRINT ''
