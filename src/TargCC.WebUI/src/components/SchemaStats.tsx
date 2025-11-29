import { Card, CardContent, CardHeader, Box, Typography, LinearProgress, Grid, Divider } from '@mui/material';
import { PieChart, BarChart } from '@mui/icons-material';

interface SchemaStatistics {
  schemaName: string;
  tableCount: number;
  percentage: number;
}

interface DataTypeStatistic {
  type: string;
  count: number;
  percentage: number;
}

interface SchemaStatsProps {
  schemas?: SchemaStatistics[];
  dataTypes?: DataTypeStatistic[];
  averageColumnsPerTable?: number;
  relationshipCount?: number;
}

const SchemaStats = ({
  schemas,
  dataTypes,
  averageColumnsPerTable = 8.5,
  relationshipCount = 42
}: SchemaStatsProps) => {
  // Mock data if none provided
  const mockSchemas: SchemaStatistics[] = [
    { schemaName: 'dbo', tableCount: 15, percentage: 62.5 },
    { schemaName: 'Sales', tableCount: 5, percentage: 20.8 },
    { schemaName: 'HR', tableCount: 3, percentage: 12.5 },
    { schemaName: 'Inventory', tableCount: 1, percentage: 4.2 }
  ];

  const mockDataTypes: DataTypeStatistic[] = [
    { type: 'VARCHAR', count: 85, percentage: 42.5 },
    { type: 'INT', count: 54, percentage: 27.0 },
    { type: 'DATETIME', count: 32, percentage: 16.0 },
    { type: 'DECIMAL', count: 18, percentage: 9.0 },
    { type: 'BIT', count: 11, percentage: 5.5 }
  ];

  const displaySchemas = schemas || mockSchemas;
  const displayDataTypes = dataTypes || mockDataTypes;

  const getSchemaColor = (index: number) => {
    const colors = ['#1976d2', '#2e7d32', '#ed6c02', '#9c27b0', '#d32f2f'];
    return colors[index % colors.length];
  };

  const getDataTypeColor = (index: number) => {
    const colors = ['#42a5f5', '#66bb6a', '#ffa726', '#ab47bc', '#ef5350'];
    return colors[index % colors.length];
  };

  return (
    <Card>
      <CardHeader 
        title="Schema Statistics"
        titleTypographyProps={{ variant: 'h6' }}
      />
      <CardContent>
        {/* Summary Stats */}
        <Grid container spacing={2} mb={3}>
          <Grid item xs={6}>
            <Box textAlign="center">
              <Typography variant="h4" color="primary">
                {averageColumnsPerTable}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Avg Columns/Table
              </Typography>
            </Box>
          </Grid>
          <Grid item xs={6}>
            <Box textAlign="center">
              <Typography variant="h4" color="success.main">
                {relationshipCount}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Relationships
              </Typography>
            </Box>
          </Grid>
        </Grid>

        <Divider sx={{ my: 2 }} />

        {/* Tables by Schema */}
        <Box mb={3}>
          <Box display="flex" alignItems="center" gap={1} mb={2}>
            <PieChart fontSize="small" color="primary" />
            <Typography variant="subtitle2" fontWeight="bold">
              Tables by Schema
            </Typography>
          </Box>
          {displaySchemas.map((schema, index) => (
            <Box key={schema.schemaName} mb={1.5}>
              <Box display="flex" justifyContent="space-between" mb={0.5}>
                <Typography variant="body2">
                  {schema.schemaName}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {schema.tableCount} ({schema.percentage.toFixed(1)}%)
                </Typography>
              </Box>
              <LinearProgress 
                variant="determinate" 
                value={schema.percentage}
                sx={{
                  height: 8,
                  borderRadius: 1,
                  backgroundColor: 'rgba(0, 0, 0, 0.08)',
                  '& .MuiLinearProgress-bar': {
                    backgroundColor: getSchemaColor(index)
                  }
                }}
              />
            </Box>
          ))}
        </Box>

        <Divider sx={{ my: 2 }} />

        {/* Data Type Distribution */}
        <Box>
          <Box display="flex" alignItems="center" gap={1} mb={2}>
            <BarChart fontSize="small" color="primary" />
            <Typography variant="subtitle2" fontWeight="bold">
              Most Common Data Types
            </Typography>
          </Box>
          {displayDataTypes.map((dataType, index) => (
            <Box key={dataType.type} mb={1.5}>
              <Box display="flex" justifyContent="space-between" mb={0.5}>
                <Typography variant="body2" fontFamily="monospace">
                  {dataType.type}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {dataType.count} ({dataType.percentage.toFixed(1)}%)
                </Typography>
              </Box>
              <LinearProgress 
                variant="determinate" 
                value={dataType.percentage}
                sx={{
                  height: 6,
                  borderRadius: 1,
                  backgroundColor: 'rgba(0, 0, 0, 0.08)',
                  '& .MuiLinearProgress-bar': {
                    backgroundColor: getDataTypeColor(index)
                  }
                }}
              />
            </Box>
          ))}
        </Box>
      </CardContent>
    </Card>
  );
};

export default SchemaStats;
