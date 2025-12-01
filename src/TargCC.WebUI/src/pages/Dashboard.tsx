/**
 * Dashboard Component
 * Main dashboard with statistics, widgets, and auto-refresh capability
 */

import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Button,
  Paper,
  Alert,
  Grid,
  Card,
  CardContent,
} from '@mui/material';
import StorageIcon from '@mui/icons-material/Storage';
import type { DashboardStats } from '../types/models';
import { SystemHealth } from '../components/SystemHealth';
import RecentGenerations from '../components/RecentGenerations';
import QuickStats from '../components/QuickStats';
import ActivityTimeline from '../components/ActivityTimeline';
import SchemaStats from '../components/SchemaStats';
import ErrorBoundary from '../components/ErrorBoundary';
import DashboardSkeleton from '../components/DashboardSkeleton';
import FadeIn from '../components/FadeIn';
import AutoRefreshControl from '../components/AutoRefreshControl';
import { useAutoRefresh } from '../hooks/useAutoRefresh';
import { useConnection } from '../hooks/useConnection';

/**
 * Main Dashboard component
 */
export const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const { selectedConnection, isLoading: connectionLoading } = useConnection();
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [autoRefreshEnabled, setAutoRefreshEnabled] = useState(false);

  const loadStats = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Simulate API delay
      await new Promise(resolve => setTimeout(resolve, 300));
      
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

  useEffect(() => {
    loadStats();
  }, []);

  // Auto-refresh hook
  const { lastRefresh, refresh } = useAutoRefresh({
    enabled: autoRefreshEnabled,
    interval: 30000, // 30 seconds
    onRefresh: loadStats
  });

  // Show loading while checking connection
  if (connectionLoading) {
    return <DashboardSkeleton />;
  }

  // Show connection prompt if no connection is selected
  if (!selectedConnection) {
    return (
      <ErrorBoundary>
        <Box>
          <Typography variant="h4" mb={3}>
            Dashboard
          </Typography>
          <Card sx={{ maxWidth: 600, mx: 'auto', mt: 8, textAlign: 'center' }}>
            <CardContent sx={{ p: 4 }}>
              <StorageIcon sx={{ fontSize: 80, color: 'primary.main', mb: 2 }} />
              <Typography variant="h5" gutterBottom>
                No Database Connection Selected
              </Typography>
              <Typography color="text.secondary" sx={{ mb: 3 }}>
                To view your dashboard and work with your database, please add and select a connection first.
              </Typography>
              <Button
                variant="contained"
                color="primary"
                size="large"
                onClick={() => navigate('/connections')}
                startIcon={<StorageIcon />}
              >
                Go to Connections
              </Button>
            </CardContent>
          </Card>
        </Box>
      </ErrorBoundary>
    );
  }

  if (loading && !stats) {
    return <DashboardSkeleton />;
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

  return (
    <ErrorBoundary>
      <Box>
        {/* Header with Auto-Refresh Control */}
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
          <Box>
            <Typography variant="h4">
              Dashboard
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Connected to: {selectedConnection.name}
            </Typography>
          </Box>
          <AutoRefreshControl
            enabled={autoRefreshEnabled}
            onToggle={setAutoRefreshEnabled}
            lastRefresh={lastRefresh}
            onManualRefresh={refresh}
          />
        </Box>

        {/* Quick Stats Cards */}
        <FadeIn delay={0}>
          <Box sx={{ mb: 4 }}>
            <QuickStats
              totalTables={stats.totalTables}
              generatedFiles={156}
              pendingUpdates={3}
              lastGeneration="2 hours ago"
            />
          </Box>
        </FadeIn>

        {/* Quick Actions */}
        <FadeIn delay={100}>
          <Paper sx={{ p: 3, mb: 4 }}>
            <Typography variant="h6" gutterBottom>
              Quick Actions
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              <Button variant="contained" color="primary" onClick={() => navigate('/generate')}>
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
        </FadeIn>

        {/* Main Content Grid */}
        <Grid container spacing={3}>
          {/* Left Column */}
          <Grid item xs={12} md={8}>
            <Grid container spacing={3}>
              {/* Recent Generations */}
              <Grid item xs={12}>
                <FadeIn delay={200}>
                  <RecentGenerations maxItems={5} />
                </FadeIn>
              </Grid>

              {/* Activity Timeline */}
              <Grid item xs={12}>
                <FadeIn delay={300}>
                  <ActivityTimeline maxItems={8} />
                </FadeIn>
              </Grid>
            </Grid>
          </Grid>

          {/* Right Column */}
          <Grid item xs={12} md={4}>
            <Grid container spacing={3}>
              {/* System Health */}
              <Grid item xs={12}>
                <FadeIn delay={400}>
                  <SystemHealth
                    cpuUsage={45}
                    memoryUsage={62}
                    diskUsage={38}
                    status="healthy"
                  />
                </FadeIn>
              </Grid>

              {/* Schema Statistics */}
              <Grid item xs={12}>
                <FadeIn delay={500}>
                  <SchemaStats />
                </FadeIn>
              </Grid>
            </Grid>
          </Grid>
        </Grid>
      </Box>
    </ErrorBoundary>
  );
};
