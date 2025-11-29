/**
 * Dashboard Component
 * Main dashboard with statistics and quick actions
 */

import { useEffect, useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Paper,
  List,
  ListItem,
  ListItemText,
  Chip,
  CircularProgress,
  Alert,
  Grid,
} from '@mui/material';
import TableChartIcon from '@mui/icons-material/TableChart';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import BugReportIcon from '@mui/icons-material/BugReport';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import type { DashboardStats } from '../types/models';
import { SystemHealth } from '../components/SystemHealth';
import RecentGenerations from '../components/RecentGenerations';
import QuickStats from '../components/QuickStats';
import ActivityTimeline from '../components/ActivityTimeline';
import SchemaStats from '../components/SchemaStats';

/**
 * Stat card component for displaying key metrics
 */
interface StatCardProps {
  title: string;
  value: string | number;
  icon: React.ReactNode;
  color: string;
}

const StatCard: React.FC<StatCardProps> = ({ title, value, icon, color }) => (
  <Card sx={{ height: '100%' }}>
    <CardContent>
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
        <Box
          sx={{
            backgroundColor: color,
            borderRadius: 1,
            p: 1,
            display: 'flex',
            mr: 2,
          }}
        >
          {icon}
        </Box>
        <Box>
          <Typography color="textSecondary" variant="body2">
            {title}
          </Typography>
          <Typography variant="h4">{value}</Typography>
        </Box>
      </Box>
    </CardContent>
  </Card>
);

/**
 * Main Dashboard component
 */
export const Dashboard: React.FC = () => {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadStats();
  }, []);

  const loadStats = async () => {
    try {
      setLoading(true);
      setError(null);
      // For now, use mock data since backend isn't ready
      const mockStats: DashboardStats = {
        totalTables: 24,
        generatedTables: 21,
        totalTests: 715,
        testCoverage: 85,
        lastGenerationTime: new Date(),
        recentActivity: [
          {
            timestamp: new Date(),
            action: 'Security Scan',
            description: 'Completed security analysis - No critical issues',
            status: 'success',
          },
          {
            timestamp: new Date(Date.now() - 3600000),
            action: 'Code Quality',
            description: 'Quality analysis: Grade B',
            status: 'warning',
          },
          {
            timestamp: new Date(Date.now() - 7200000),
            action: 'Generation',
            description: 'Generated Customer entity and repository',
            status: 'success',
          },
        ],
      };
      setStats(mockStats);
      // Real API call (when backend is ready):
      // const data = await apiService.getDashboardStats();
      // setStats(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load dashboard stats');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '80vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 2 }}>
        {error}
      </Alert>
    );
  }

  if (!stats) {
    return null;
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'success':
        return 'success';
      case 'warning':
        return 'warning';
      case 'error':
        return 'error';
      default:
        return 'default';
    }
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>

      {/* Quick Stats Cards */}
      <Box sx={{ mb: 4 }}>
        <QuickStats
          totalTables={stats.totalTables}
          generatedFiles={156}
          pendingUpdates={3}
          lastGeneration="2 hours ago"
        />
      </Box>

      {/* Quick Actions */}
      <Paper sx={{ p: 3, mb: 4 }}>
        <Typography variant="h6" gutterBottom>
          Quick Actions
        </Typography>
        <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
          <Button variant="contained" color="primary">
            Generate All
          </Button>
          <Button variant="outlined" color="primary">
            Analyze Security
          </Button>
          <Button variant="outlined" color="primary">
            Check Quality
          </Button>
          <Button variant="outlined" color="primary">
            AI Chat
          </Button>
        </Box>
      </Paper>

      {/* Main Content Grid */}
      <Grid container spacing={3}>
        {/* Left Column */}
        <Grid item xs={12} md={8}>
          <Grid container spacing={3}>
            {/* Recent Generations */}
            <Grid item xs={12}>
              <RecentGenerations maxItems={5} />
            </Grid>

            {/* Activity Timeline */}
            <Grid item xs={12}>
              <ActivityTimeline maxItems={8} />
            </Grid>
          </Grid>
        </Grid>

        {/* Right Column */}
        <Grid item xs={12} md={4}>
          <Grid container spacing={3}>
            {/* System Health */}
            <Grid item xs={12}>
              <SystemHealth
                cpuUsage={45}
                memoryUsage={62}
                diskUsage={38}
                status="healthy"
              />
            </Grid>

            {/* Schema Statistics */}
            <Grid item xs={12}>
              <SchemaStats />
            </Grid>
          </Grid>
        </Grid>
      </Grid>
    </Box>
  );
};
