import { Paper, Grid, Typography, Box, LinearProgress, Chip } from '@mui/material';
import TableChartIcon from '@mui/icons-material/TableChart';
import ViewColumnIcon from '@mui/icons-material/ViewColumn';
import AccountTreeIcon from '@mui/icons-material/AccountTree';
import LabelIcon from '@mui/icons-material/Label';
import type { DatabaseSchema } from '../../types/schema';

/**
 * Props for SchemaStats component
 */
interface SchemaStatsProps {
  /** Database schema to display statistics for */
  schema: DatabaseSchema;
}

/**
 * Props for StatCard component
 */
interface StatCardProps {
  /** Label for the statistic */
  label: string;
  /** Numeric value */
  value: number;
  /** Optional subtitle/secondary text */
  subtitle?: string;
  /** Icon to display */
  icon: React.ReactNode;
}

/**
 * Individual statistic card component
 */
function StatCard({ label, value, subtitle, icon }: StatCardProps) {
  return (
    <Paper
      sx={{
        p: 2,
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        textAlign: 'center',
        height: '100%',
      }}
    >
      <Box sx={{ color: 'primary.main', mb: 1 }}>{icon}</Box>
      <Typography variant="h4" component="div" fontWeight="bold">
        {value.toLocaleString()}
      </Typography>
      <Typography variant="body2" color="text.secondary">
        {label}
      </Typography>
      {subtitle && (
        <Chip label={subtitle} size="small" color="primary" sx={{ mt: 1 }} />
      )}
    </Paper>
  );
}

/**
 * Schema Statistics Component
 * Displays comprehensive statistics about the database schema
 */
export default function SchemaStats({ schema }: SchemaStatsProps) {
  // Calculate basic statistics
  const totalTables = schema.tables.length;
  const totalColumns = schema.tables.reduce((sum, t) => sum + t.columns.length, 0);
  const totalRelationships = schema.relationships.length;
  const targccTables = schema.tables.filter((t) => t.hasTargCCColumns).length;
  const avgColumnsPerTable = totalTables > 0 ? (totalColumns / totalTables).toFixed(1) : '0';

  // Calculate data type distribution
  const dataTypes = schema.tables.flatMap((t) => t.columns.map((c) => c.type));
  const typeCount = dataTypes.reduce(
    (acc, type) => {
      acc[type] = (acc[type] || 0) + 1;
      return acc;
    },
    {} as Record<string, number>
  );

  // Sort by count and take top 5
  const topTypes = Object.entries(typeCount)
    .sort(([, a], [, b]) => b - a)
    .slice(0, 5);

  // Calculate TargCC percentage
  const targccPercentage = totalTables > 0 
    ? Math.round((targccTables / totalTables) * 100) 
    : 0;

  return (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom sx={{ mb: 3 }}>
        Schema Statistics
      </Typography>

      {/* Main Statistics Grid */}
      <Grid container spacing={2} sx={{ mb: 4 }}>
        <Grid size={{ xs: 6, md: 3 }}>
          <StatCard
            label="Tables"
            value={totalTables}
            icon={<TableChartIcon fontSize="large" />}
          />
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <StatCard
            label="Columns"
            value={totalColumns}
            subtitle={`Avg: ${avgColumnsPerTable} per table`}
            icon={<ViewColumnIcon fontSize="large" />}
          />
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <StatCard
            label="Relationships"
            value={totalRelationships}
            icon={<AccountTreeIcon fontSize="large" />}
          />
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <StatCard
            label="TargCC Tables"
            value={targccTables}
            subtitle={`${targccPercentage}% of total`}
            icon={<LabelIcon fontSize="large" />}
          />
        </Grid>
      </Grid>

      {/* Data Type Distribution */}
      {topTypes.length > 0 && (
        <Box>
          <Typography variant="subtitle2" gutterBottom sx={{ mb: 2 }}>
            Top Data Types
          </Typography>
          {topTypes.map(([type, count]) => {
            const percentage = totalColumns > 0 
              ? ((count / totalColumns) * 100).toFixed(1) 
              : '0';
            
            return (
              <Box key={type} sx={{ mb: 2 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                  <Typography variant="body2" fontWeight="medium">
                    {type}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {count} ({percentage}%)
                  </Typography>
                </Box>
                <LinearProgress
                  variant="determinate"
                  value={parseFloat(percentage)}
                  sx={{ height: 8, borderRadius: 1 }}
                />
              </Box>
            );
          })}
        </Box>
      )}
    </Paper>
  );
}
