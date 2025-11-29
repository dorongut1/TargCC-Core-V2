import { Box, Grid, Skeleton, Card, CardContent, Paper } from '@mui/material';

/**
 * Loading skeleton for the Dashboard page.
 * Shows placeholder content while data is being fetched.
 */
const DashboardSkeleton = () => {
  return (
    <Box>
      {/* Page Title Skeleton */}
      <Skeleton
        variant="text"
        width={200}
        height={48}
        sx={{ mb: 3 }}
      />

      {/* QuickStats Skeletons - 4 stat cards */}
      <Grid container spacing={2} mb={4}>
        {[1, 2, 3, 4].map((i) => (
          <Grid item xs={12} sm={6} md={3} key={i}>
            <Card>
              <CardContent>
                <Skeleton variant="text" width="60%" height={24} />
                <Skeleton variant="text" width="40%" height={56} sx={{ mt: 1 }} />
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Main Content Grid */}
      <Grid container spacing={3}>
        {/* Left Column - 2 widgets */}
        <Grid item xs={12} md={8}>
          <Grid container spacing={3}>
            {/* Recent Generations Widget */}
            <Grid item xs={12}>
              <Paper sx={{ p: 2 }}>
                <Skeleton variant="text" width={180} height={32} sx={{ mb: 2 }} />
                {[1, 2, 3, 4].map((i) => (
                  <Box key={i} sx={{ mb: 2, display: 'flex', gap: 2 }}>
                    <Skeleton variant="circular" width={40} height={40} />
                    <Box sx={{ flex: 1 }}>
                      <Skeleton variant="text" width="70%" />
                      <Skeleton variant="text" width="40%" />
                    </Box>
                  </Box>
                ))}
              </Paper>
            </Grid>

            {/* Activity Timeline Widget */}
            <Grid item xs={12}>
              <Paper sx={{ p: 2 }}>
                <Skeleton variant="text" width={160} height={32} sx={{ mb: 2 }} />
                {[1, 2, 3].map((i) => (
                  <Box key={i} sx={{ mb: 3, display: 'flex', gap: 2 }}>
                    <Skeleton variant="circular" width={32} height={32} />
                    <Box sx={{ flex: 1 }}>
                      <Skeleton variant="text" width="60%" />
                      <Skeleton variant="text" width="30%" />
                    </Box>
                  </Box>
                ))}
              </Paper>
            </Grid>
          </Grid>
        </Grid>

        {/* Right Column - 2 widgets */}
        <Grid item xs={12} md={4}>
          <Grid container spacing={3}>
            {/* System Health Widget */}
            <Grid item xs={12}>
              <Paper sx={{ p: 2 }}>
                <Skeleton variant="text" width={140} height={32} sx={{ mb: 2 }} />
                {[1, 2, 3, 4].map((i) => (
                  <Box key={i} sx={{ mb: 2 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                      <Skeleton variant="text" width="40%" />
                      <Skeleton variant="text" width="20%" />
                    </Box>
                    <Skeleton variant="rectangular" height={8} sx={{ borderRadius: 1 }} />
                  </Box>
                ))}
              </Paper>
            </Grid>

            {/* Schema Stats Widget */}
            <Grid item xs={12}>
              <Paper sx={{ p: 2 }}>
                <Skeleton variant="text" width={150} height={32} sx={{ mb: 2 }} />
                {[1, 2, 3].map((i) => (
                  <Box key={i} sx={{ mb: 2 }}>
                    <Skeleton variant="text" width="50%" />
                    <Skeleton variant="rectangular" height={6} sx={{ mt: 1, borderRadius: 1 }} />
                  </Box>
                ))}
              </Paper>
            </Grid>
          </Grid>
        </Grid>
      </Grid>
    </Box>
  );
};

export default DashboardSkeleton;
