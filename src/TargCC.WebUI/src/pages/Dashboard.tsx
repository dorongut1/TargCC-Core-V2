/**
 * Dashboard Component
 * Main dashboard with statistics and quick actions
 */

import { useEffect, useState } from 'react';
import {
  Box,
  Grid,
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
} from '@mui/material';
import TableChartIcon from '@mui/icons-material/TableChart';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import BugReportIcon from '@mui/icons-material/BugReport';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import { apiService } from '../services/api';
import type { DashboardStats } from '../types/models';

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
        totalTables: 12,
        generatedTables: 8,
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

      {/* Statistics Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Total Tables"
            value={stats.totalTables}
            icon={<TableChartIcon sx={{ color: 'white' }} />}
            color="#1976d2"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Generated"
            value={stats.generatedTables}
            icon={<CheckCircleIcon sx={{ color: 'white' }} />}
            color="#2e7d32"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Tests"
            value={stats.totalTests}
            icon={<BugReportIcon sx={{ color: 'white' }} />}
            color="#ed6c02"
          />
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <StatCard
            title="Coverage"
            value={`${stats.testCoverage}%`}
            icon={<TrendingUpIcon sx={{ color: 'white' }} />}
            color="#9c27b0"
          />
        </Grid>
      </Grid>

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

      {/* Recent Activity */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Recent Activity
        </Typography>
        <List>
          {stats.recentActivity.map((activity, index) => (
            <ListItem key={index} divider={index < stats.recentActivity.length - 1}>
              <ListItemText
                primary={activity.action}
                secondary={
                  <>
                    {activity.description}
                    <br />
                    {activity.timestamp.toLocaleString()}
                  </>
                }
              />
              <Chip
                label={activity.status}
                color={getStatusColor(activity.status) as any}
                size="small"
              />
            </ListItem>
          ))}
        </List>
      </Paper>
    </Box>
  );
};
