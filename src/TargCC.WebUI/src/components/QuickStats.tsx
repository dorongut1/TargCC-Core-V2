import { Card, CardContent, Grid, Typography, Box } from '@mui/material';
import { TableChart, Code, Update, Schedule } from '@mui/icons-material';

interface QuickStat {
  label: string;
  value: string | number;
  icon: React.ReactNode;
  color: string;
}

interface QuickStatsProps {
  totalTables?: number;
  generatedFiles?: number;
  pendingUpdates?: number;
  lastGeneration?: string;
}

const QuickStats = ({
  totalTables = 24,
  generatedFiles = 156,
  pendingUpdates = 3,
  lastGeneration = '2 hours ago'
}: QuickStatsProps) => {
  const stats: QuickStat[] = [
    {
      label: 'Total Tables',
      value: totalTables,
      icon: <TableChart fontSize="large" />,
      color: '#1976d2' // primary blue
    },
    {
      label: 'Generated Files',
      value: generatedFiles,
      icon: <Code fontSize="large" />,
      color: '#2e7d32' // success green
    },
    {
      label: 'Pending Updates',
      value: pendingUpdates,
      icon: <Update fontSize="large" />,
      color: pendingUpdates > 0 ? '#ed6c02' : '#2e7d32' // warning orange or green
    },
    {
      label: 'Last Generation',
      value: lastGeneration,
      icon: <Schedule fontSize="large" />,
      color: '#757575' // gray
    }
  ];

  return (
    <Grid container spacing={2}>
      {stats.map((stat, index) => (
        <Grid item xs={12} sm={6} md={3} key={index}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="text.secondary" variant="body2" gutterBottom>
                    {stat.label}
                  </Typography>
                  <Typography variant="h5" component="div">
                    {stat.value}
                  </Typography>
                </Box>
                <Box 
                  sx={{ 
                    color: stat.color,
                    opacity: 0.7
                  }}
                >
                  {stat.icon}
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      ))}
    </Grid>
  );
};

export default QuickStats;
